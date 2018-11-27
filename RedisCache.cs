using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PTrust.Domain.Monads;
using StackExchange.Redis;

namespace PTrust.Infrastructure.Redis
{
    public static class RedisCache
    {
        private static Lazy<ConnectionMultiplexer> LazyConnection => new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(ConfigurationKeys.Redis.GetAppSettingValue()));
        private static ConnectionMultiplexer _connection;
        private static string ApplicationPrefix => ConfigurationKeys.RedisApplicationPrefix.GetAppSettingValue();
        private static IDatabase _cache;
        public static IDatabase Cache => _cache ?? InitializeCache();
        public static TimeSpan DefaultExpiration => GetDefaultExpiry();

        public static string CreateCacheId(string prefixKey, string uniqueIdentifier, string suffix = null)
        {
            var s = $"{ApplicationPrefix}-{prefixKey}:{uniqueIdentifier}";
            if (!string.IsNullOrEmpty(suffix))
            {
                s += $"|{suffix}";
            }

            return s.ToUpper();
        }

        public static void CacheItem<T>(string cacheId, T value, TimeSpan? expiration)
        {
            Cache.StringSet(cacheId, value.ToJson(), expiration);
        }

        public static Maybe<T> GetCachedItem<T>(string cacheId)
        {
            var s = Cache.StringGet(cacheId);

            try
            {
                var obj = s.ToObject<T>();
                return obj.ToMaybe();
            }
            catch (Exception e)
            {
                return Maybe.Empty<T>();
            }

        }

        /// <summary>
        /// You may place a wildcard '*' before and/or after your pattern.
        /// Note: This may come with a performance penalty.
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static List<RedisKey> SearchKeys(string pattern)
        {
            var endpoint = Cache.IdentifyEndpoint();
            var s = _connection.GetServer(endpoint);
            var keys = s.Keys(Cache.Database, pattern: pattern);
            return keys.ToList();
        }

        public static void DeleteCachedItem(string cacheId)
        {
            Cache.KeyDelete(cacheId);
        }

        private static IDatabase InitializeCache()
        {
            _connection = LazyConnection.Value;
            _cache = _connection.GetDatabase();
            return _cache;
        }

        private static TimeSpan GetDefaultExpiry()
        {
            var expiryStr = ConfigurationKeys.RedisDefaultExpiry.GetAppSettingValue();

            int seconds;
            bool isNumber = int.TryParse(expiryStr, out seconds);

            return TimeSpan.FromSeconds(isNumber ? seconds : 600);
        }

    }
}
