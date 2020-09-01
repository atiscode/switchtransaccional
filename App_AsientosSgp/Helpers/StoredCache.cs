using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace App_Mundial_Miles.Helpers
{
    public static class StoredCache
    {
        private const string CacheKey = "TokenSAFI";

        public static string GetTokenCacheStored()
        {
            ObjectCache cache = MemoryCache.Default;

            if (cache.Contains(CacheKey))
                return (string)cache.Get(CacheKey);
            else
                return string.Empty;
        }

        public static void UpdateTokenCacheStored(string token)
        {
            ObjectCache cache = MemoryCache.Default;

            // Store data in the cache
            CacheItemPolicy cacheItemPolicy = new CacheItemPolicy()
            {
                AbsoluteExpiration = DateTime.Now.AddHours(20.0)
            };

            cache.Add(CacheKey, token, cacheItemPolicy);
        }
    }
}