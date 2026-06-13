namespace Caching.Cache
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T> (string key);
        Task SetAsync<T>(string key, T Value, TimeSpan? expiration=null);
        Task RemoveAsync(string key);
    }
}
