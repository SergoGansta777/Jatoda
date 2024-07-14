using Jatoda.Infrastructure.CacheService.Interfaces;
using Jatoda.Infrastructure.CacheService.Repositories.Interfaces;

namespace Jatoda.Infrastructure.CacheService;

public class CacheService(ICacheRepository cacheRepository) : ICacheService
{
    public async Task<T?> GetFromCacheAsync<T>(string key)
    {
        return await cacheRepository.GetFromCacheAsync<T>(key);
    }

    public async Task SetCacheAsync<T>(string key, T value, TimeSpan expiration)
    {
        await cacheRepository.SetCacheAsync(key, value, expiration);
    }

    public async Task RemoveFromCacheAsync(string key)
    {
        await cacheRepository.RemoveFromCacheAsync(key);
    }

    public async Task<T> GetOrCreateCacheAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration)
    {
        var cachedValue = await cacheRepository.GetFromCacheAsync<T>(key);

        if (cachedValue is not null)
        {
            return cachedValue;
        }

        var value = await factory();
        await cacheRepository.SetCacheAsync(key, value, expiration);

        return value;
    }
}