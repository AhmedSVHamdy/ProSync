using Core.ServiceContracts;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Core.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var cachedValue = await _cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(cachedValue))
                return default;

            return JsonSerializer.Deserialize<T>(cachedValue);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            var serializedValue = JsonSerializer.Serialize(value);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            await _cache.SetStringAsync(key, serializedValue, options);
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }
}
