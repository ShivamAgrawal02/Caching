
using Microsoft.Extensions.Caching.Memory;

namespace Caching.Cache
{
    public class CacheService : ICacheService
    {
        private IMemoryCache _memoryCache;
        public CacheService(IMemoryCache memoryCache)
        {
           _memoryCache = memoryCache;
        }
        public Task<T?> GetAsync<T>(string key)
        {
            var data = _memoryCache.TryGetValue(key, out T? value);
            return Task.FromResult(value);
        }

        public Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }

        public Task SetAsync<T>(string key, T Value, TimeSpan? expiration = null)
        {
            var options = new MemoryCacheEntryOptions();
            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration.Value;
                options.SlidingExpiration = expiration.Value / 2;
            }
            _memoryCache.Set(key, Value, options);
            return Task.CompletedTask;
        }
    }
}
