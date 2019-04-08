using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;

namespace HeadNonSub {

    public static class Cache {

        private static MemoryCache _Cache = MemoryCache.Default;

        /// <summary>
        /// All stalkers.
        /// </summary>
        public static ConcurrentBag<(ulong serverId, ulong userId, ulong stalkingUserId)> Stalkers { get; private set; }

        /// <summary>
        /// Set <see cref="Stalkers"/>.
        /// </summary>
        /// <param name="stalkers">List of stalkers.</param>
        public static void SetStalkers(ConcurrentBag<(ulong serverId, ulong userId, ulong stalkingUserId)> stalkers) => Stalkers = stalkers;

        /// <summary>
        /// Add an entry to the cache.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry to add.</param>
        /// <param name="item">The data for the cache entry.</param>
        /// <param name="expirationMin">Number of minutes the cache will live. If null it will never expire.</param>
        public static void Add(string key, object item, int? expirationMin = 5) {
            if (expirationMin.HasValue) {
                _Cache.Add(key, item, DateTimeOffset.Now.AddMinutes(expirationMin.Value));
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
                _Cache.Add(key, item, DateTimeOffset.Now.AddMinutes(expirationMin.Value));
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

            // Load all stalkers to cache.
            //SetStalkers(Database.DatabaseManager.Stalking.GetAll());

            LoggingManager.Log.Info($"Loaded {_Cache.GetCount()} items into cache");
        }

    }

}
