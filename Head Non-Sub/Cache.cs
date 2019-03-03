using System;
using System.Runtime.Caching;

namespace HeadNonSub {

    public static class Cache {

        private static MemoryCache _Cache = MemoryCache.Default;

        /// <summary>
        /// Adds a cache entry into the cache using the specified key and a value. If item already exists it will be returned.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry to add.</param>
        /// <param name="item">The data for the cache entry.</param>
        public static object AddOrGetExisting(string key, object item, int expirationMin = 5) => _Cache.AddOrGetExisting(key, item, DateTimeOffset.Now.AddMinutes(expirationMin));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry to add.</param>
        /// <param name="item">The data for the cache entry.</param>
        public static void Add(string key, object item, int expirationMin = 5) => _Cache.Add(key, item, DateTimeOffset.Now.AddMinutes(expirationMin));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        public static object Get(string key) => _Cache.Get(key);

    }

}
