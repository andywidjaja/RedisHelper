using System;
using System.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace PTrust.Infrastructure.Redis
{
    internal static class Extensions
    {
        private static JsonSerializerSettings _settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        public static string GetAppSettingValue(this string key)
        {
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[key]))
            {
                throw new ArgumentNullException(key, $"Could not find the key in AppSettings for: {key}. Make sure the AppSettings has the value defined");
            }

            return ConfigurationManager.AppSettings[key];
        }
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj, _settings);
        }

        public static T ToObject<T>(this RedisValue redisValue)
        {
            return JsonConvert.DeserializeObject<T>(redisValue, _settings);
        }
    }
}
