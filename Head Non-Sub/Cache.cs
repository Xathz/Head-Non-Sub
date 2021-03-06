﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using HeadNonSub.Extensions;
using HeadNonSub.Properties;

namespace HeadNonSub {

    public static class Cache {

        private static readonly MemoryCache _Cache = MemoryCache.Default;

        public static IReadOnlyCollection<string> TLDs { get; private set; }

        /// <summary>
        /// Add an entry to the cache.
        /// </summary>
        /// <param name="key">Unique identifier for the cache entry to add.</param>
        /// <param name="item">Data for the cache entry.</param>
        /// <param name="expirationMin">Number of minutes the cache will live. If <see langword="null"/> it will never expire.</param>
        public static void Add(string key, object item, int? expirationMin = 5) {
            if (expirationMin.HasValue) {
                _Cache.Add(key, item, DateTimeOffset.UtcNow.AddMinutes(expirationMin.Value));
                LoggingManager.Log.Debug($"Added '{key}' and will expire in {expirationMin} minute(s)");
            } else {
                _Cache.Add(key, item, ObjectCache.InfiniteAbsoluteExpiration);
                LoggingManager.Log.Debug($"Added '{key}' and will never expire");
            }
        }

        /// <summary>
        /// Add or update an entry to the cache.
        /// </summary>
        /// <param name="key">Unique identifier for the cache entry to update.</param>
        /// <param name="item">Data for the cache entry.</param>
        /// <param name="expirationMin">Number of minutes the cache will live. If <see langword="null"/> it will never expire.</param>
        public static void AddOrUpdate(string key, object item, int? expirationMin = 5) {
            bool wasUpdated = _Cache.Contains(key);

            Remove(key);
            Add(key, item, expirationMin);

            if (wasUpdated) {
                LoggingManager.Log.Debug($"Removed and readded '{key}'");
            }
        }

        /// <summary>
        /// Remove an entry from the cache.
        /// </summary>
        /// <param name="key">Unique identifier for the cache entry to update.</param>
        public static void Remove(string key) {
            if (_Cache.Contains(key)) {
                _Cache.Remove(key);
                LoggingManager.Log.Debug($"Removed '{key}'");
            } else {
                LoggingManager.Log.Debug($"Attempted to remove '{key}' but the key does not exist");
            }
        }

        /// <summary>
        /// Get a entry from the cache.
        /// </summary>
        /// <param name="key">Unique identifier for the cache entry to get.</param>
        public static object Get(string key) {
            if (_Cache.Contains(key)) {
                LoggingManager.Log.Debug($"Retrieved '{key}'");
                return _Cache.Get(key);
            } else {
                LoggingManager.Log.Debug($"Attempted to retrieve '{key}' but the key does not exist");
                return null;
            }
        }

        /// <summary>
        /// Get a comma separated list of all keys in the cache.
        /// </summary>
        public static string ListKeys() => string.Join(", ", _Cache.Select(x => x.Key).ToList());

        /// <summary>
        /// Get a entry from the cache as a <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        public static MemoryStream GetStream(string key) {
            if (!_Cache.Contains(key)) {
                return null;
            }

            MemoryStream memoryStream = new MemoryStream(256);

            // Copy to a new stream becuase Discord will close this when used
            MemoryStream fromCache = _Cache.Get(key) as MemoryStream;
            fromCache.CopyTo(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            // Reset position of the stream in the cache
            fromCache.Seek(0, SeekOrigin.Begin);
            _Cache.Set(key, fromCache, ObjectCache.InfiniteAbsoluteExpiration);

            return memoryStream;
        }

        /// <summary>
        /// Load all items in <see cref="Constants.ContentDirectory"/> and <see cref="Constants.TemplatesDirectory"/> into the cache.
        /// </summary>
        public static async Task LoadContentAsync() {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(Constants.ContentDirectory, "*.*", SearchOption.TopDirectoryOnly));
            files.AddRange(Directory.GetFiles(Constants.TemplatesDirectory, "*.*", SearchOption.TopDirectoryOnly));
            files.AddRange(Directory.GetFiles(Constants.TwemojiDirectory, "*.png", SearchOption.TopDirectoryOnly));

            foreach (string file in files) {
                FileInfo fileInfo = new FileInfo(file);
                string key = fileInfo.Name;

                if (fileInfo.FullName.Contains(Constants.TwemojiDirectory)) {
                    key = $"twemoji_{fileInfo.Name}";
                }

                MemoryStream memoryStream = new MemoryStream();
                using (FileStream fileStream = File.OpenRead(fileInfo.FullName)) {
                    fileStream.CopyTo(memoryStream);
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                _Cache.Add(key, memoryStream, ObjectCache.InfiniteAbsoluteExpiration);
            }

            // Download and parse top level domains
            await DownloadTLDsAsync();

            LoggingManager.Log.Info($"Loaded {_Cache.GetCount()} items into cache");
        }

        private static async Task DownloadTLDsAsync() {
            try {
                Task<string> download = Http.SendRequestAsync("https://data.iana.org/TLD/tlds-alpha-by-domain.txt");
                string data = await download;

                if (download.IsCompletedSuccessfully) {
                    List<string> lines = data.SplitByNewLines();
                    HashSet<string> domains = new HashSet<string>();
                    foreach (string line in lines) {
                        if (!line.StartsWith("#")) {
                            domains.Add(line.ToLower());
                        }
                    }

                    if (domains.Count == 0) {
                        throw new ArgumentException("Collection is empty", nameof(domains));
                    }

                    TLDs = domains.ToList().AsReadOnly();

                    LoggingManager.Log.Info($"Downloaded and parsed {TLDs.Count} top level domains");
                } else {
                    throw download.Exception;
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);

                List<string> lines = Resources.TLDs.SplitByNewLines();
                HashSet<string> domains = new HashSet<string>();
                foreach (string line in lines) {
                    if (!line.StartsWith("#")) {
                        domains.Add(line.ToLower());
                    }
                }

                TLDs = domains.ToList().AsReadOnly();

                LoggingManager.Log.Info($"Parsed from resources {TLDs.Count} top level domains");
            }
        }

    }

}
