using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HeadNonSub.Extensions;
using HeadNonSub.Properties;
using Microsoft.Extensions.Caching.Memory;

namespace HeadNonSub {

    public static class Cache {

        private static IMemoryCache _Cache = new MemoryCache(new MemoryCacheOptions());
        private static HashSet<string> _CacheKeys = new HashSet<string>();

        public static IReadOnlyCollection<string> TLDs { get; private set; }

        /// <summary>
        /// Add an entry to the cache.
        /// </summary>
        /// <param name="key">Unique identifier for the cache entry to add.</param>
        /// <param name="item">Data for the cache entry.</param>
        /// <param name="expirationMin">Number of minutes the cache will live. If <see langword="null"/> it will never expire.</param>
        public static void Add(string key, object item, int? expirationMin = 5) {
            _CacheKeys.Add(key);
            if (expirationMin.HasValue) {
                MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(expirationMin.Value));

                _Cache.Set(key, item, cacheOptions);
                LoggingManager.Log.Debug($"Added '{key}' and will expire in {expirationMin} minute(s)");
            } else {
                _Cache.Set(key, item);
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
            bool wasUpdated = _Cache.TryGetValue(key, out object _);

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
            if (_Cache.TryGetValue(key, out object _)) {
                _Cache.Remove(key);
                _CacheKeys.Remove(key);

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
            if (_Cache.TryGetValue(key, out object value)) {
                LoggingManager.Log.Debug($"Retrieved '{key}'");

                return value;
            } else {
                LoggingManager.Log.Debug($"Attempted to retrieve '{key}' but the key does not exist");
                return null;
            }
        }

        /// <summary>
        /// Get a comma separated list of all keys in the cache.
        /// </summary>
        public static string ListKeys() => string.Join(", ", _CacheKeys.ToList());

        /// <summary>
        /// Get a entry from the cache as a new read only <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        public static MemoryStream GetStream(string key) {
            if (_Cache.TryGetValue(key, out object cachedValue) && cachedValue is byte[] data) {
                // New stream each call so commands running at the same time don't share a position
                return new MemoryStream(data, false);
            }

            return null;
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

                byte[] data = await File.ReadAllBytesAsync(fileInfo.FullName);
                _CacheKeys.Add(key);
                _Cache.Set(key, data);
            }

            // Download and parse top level domains
            await DownloadTLDsAsync();

            LoggingManager.Log.Info($"Loaded {_CacheKeys.Count} items into cache");
        }

        private static async Task DownloadTLDsAsync() {
            try {
                Task<string> download = Http.SendRequestAsync("https://data.iana.org/TLD/tlds-alpha-by-domain.txt");
                string data = await download;

                if (download.IsCompletedSuccessfully) {
                    List<string> lines = data.SplitByNewLines();
                    HashSet<string> domains = new HashSet<string>();
                    foreach (string line in lines) {
                        if (!line.StartsWith('#')) {
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
                    if (!line.StartsWith('#')) {
                        domains.Add(line.ToLower());
                    }
                }

                TLDs = domains.ToList().AsReadOnly();

                LoggingManager.Log.Info($"Parsed from resources {TLDs.Count} top level domains");
            }
        }

    }

}
