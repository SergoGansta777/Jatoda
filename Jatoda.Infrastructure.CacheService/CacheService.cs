using Jatoda.Infrastructure.CacheService.Interfaces;
using Jatoda.Infrastructure.CacheService.Repositories.Interfaces;

namespace Jatoda.Infrastructure.CacheService;

public class CacheService : ICacheService
{
    private readonly ICacheRepository _cacheRepository;

    public CacheService(ICacheRepository cacheRepository)
    {
        _cacheRepository = cacheRepository;
    }

    public async Task<T?> GetFromCacheAsync<T>(string key)
    {
        return await _cacheRepository.GetFromCacheAsync<T>(key);
    }

    public async Task SetCacheAsync<T>(string key, T value, TimeSpan expiration)
    {
        await _cacheRepository.SetCacheAsync(key, value, expiration);
    }

    public async Task RemoveFromCacheAsync(string key)
    {
        await _cacheRepository.RemoveFromCacheAsync(key);
    }

    public async Task<T> GetOrCreateCacheAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration)
    {
        var cachedValue = await _cacheRepository.GetFromCacheAsync<T>(key);

        if (cachedValue is not null)
        {
            return cachedValue;
        }

        var value = await factory();
        await _cacheRepository.SetCacheAsync(key, value, expiration);

        return value;
    }
}