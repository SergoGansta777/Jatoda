namespace Jatoda.Infrastructure.CacheService.Repositories.Interfaces;

public interface ICacheRepository
{
    Task<T?> GetFromCacheAsync<T>(string key);
    Task SetCacheAsync<T>(string key, T value, TimeSpan expiration);
    Task RemoveFromCacheAsync(string key);
}