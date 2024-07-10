namespace Jatoda.Infrastructure.CacheService.Interfaces;

public interface ICacheService
{
    Task<T?> GetFromCacheAsync<T>(string key);
    Task SetCacheAsync<T>(string key, T value, TimeSpan expiration);
    Task RemoveFromCacheAsync(string key);
    Task<T> GetOrCreateCacheAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration);
}