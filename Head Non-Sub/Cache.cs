using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using HeadNonSub.Extensions;
using HeadNonSub.Properties;

namespace HeadNonSub {

    public static class Cache {

        private static MemoryCache _Cache = MemoryCache.Default;

        public static IReadOnlyCollection<string> TLDs { get; private set; }

        /// <summary>
        /// Add an entry to the cache.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry to add.</param>
        /// <param name="item">The data for the cache entry.</param>
        /// <param name="expirationMin">Number of minutes the cache will live. If null it will never expire.</param>
        public static void Add(string key, object item, int? expirationMin = 5) {
            if (expirationMin.HasValue) {
                _Cache.Add(key, item, DateTimeOffset.UtcNow.AddMinutes(expirationMin.Value));
                LoggingManager.Log.Info($"Added '{key}' to the cache and will expire in {expirationMin} minute(s)");
            } else {
                _Cache.Add(key, item, ObjectCache.InfiniteAbsoluteExpiration);
                LoggingManager.Log.Info($"Added '{key}' to the cache and will never expire");
            }
        }

        /// <summary>
        /// Add or update an entry to/in the cache.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry to add.</param>
        /// <param name="item">The data for the cache entry.</param>
        /// <param name="expirationMin">Number of minutes the cache will live. If null it will never expire.</param>
        public static void AddOrUpdate(string key, object item, int? expirationMin = 5) {
            if (_Cache.Contains(key)) {
                _Cache.Remove(key);
                LoggingManager.Log.Info($"Removed '{key}' from the cache");
            }

            if (expirationMin.HasValue) {
                _Cache.Add(key, item, DateTimeOffset.UtcNow.AddMinutes(expirationMin.Value));
                LoggingManager.Log.Info($"Added '{key}' to the cache and will expire in {expirationMin} minute(s)");
            } else {
                _Cache.Add(key, item, ObjectCache.InfiniteAbsoluteExpiration);
                LoggingManager.Log.Info($"Added '{key}' to the cache and will never expire");
            }
        }

        /// <summary>
        /// Get a entry from the cache.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        public static object Get(string key) {
            object entry = _Cache.Get(key);

            if (entry is null) {
                LoggingManager.Log.Info($"Attempted to retrieved '{key}' from cache but is null");
            } else {
                LoggingManager.Log.Info($"Retrieved '{key}' from cache");
            }

            return entry;
        }

        /// <summary>
        /// Get a entry from the cache as a <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        public static MemoryStream GetStream(string key) {
            MemoryStream memoryStream = new MemoryStream();

            // Copy to a new stream becuase Discord will close this when used
            MemoryStream fromCache = (_Cache.Get(key) as MemoryStream);
            fromCache.CopyTo(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            // Reset position of the stream in the cache
            fromCache.Seek(0, SeekOrigin.Begin);
            _Cache.Set(key, fromCache, ObjectCache.InfiniteAbsoluteExpiration);

            return memoryStream;
        }

        /// <summary>
        /// Load all the items in the <see cref="Constants.ContentDirectory"/> and <see cref="Constants.TemplatesDirectory"/> into the cache.
        /// </summary>
        public static void LoadContent() {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(Constants.ContentDirectory, "*.*", SearchOption.TopDirectoryOnly));
            files.AddRange(Directory.GetFiles(Constants.TemplatesDirectory, "*.*", SearchOption.TopDirectoryOnly));

            foreach (string file in files) {
                FileInfo fileInfo = new FileInfo(file);

                MemoryStream memoryStream = new MemoryStream();
                using (FileStream fileStream = File.OpenRead(fileInfo.FullName)) {
                    fileStream.CopyTo(memoryStream);
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                _Cache.Add(fileInfo.Name, memoryStream, ObjectCache.InfiniteAbsoluteExpiration);
            }

            // Download and parse top level domains
            DownloadTLDs();

            LoggingManager.Log.Info($"Loaded {_Cache.GetCount()} items into cache");
        }

        private static void DownloadTLDs() {
            try {
                WebClient webClient = new WebClient();
                string list = webClient.DownloadString("https://data.iana.org/TLD/tlds-alpha-by-domain.txt");

                if (string.IsNullOrWhiteSpace(list)) {
                    throw new ArgumentNullException("list");
                }

                List<string> lines = list.SplitByNewLines();
                HashSet<string> domains = new HashSet<string>();
                foreach (string line in lines) {
                    if (!line.StartsWith("#")) {
                        domains.Add(line.ToLower());
                    }
                }

                if (domains.Count == 0) {
                    throw new ArgumentNullException("domains");
                }

                TLDs = domains.ToList().AsReadOnly();

                LoggingManager.Log.Info($"Downloaded and parsed {TLDs.Count} top level domains");
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
