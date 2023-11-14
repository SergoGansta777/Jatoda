using JatodaBackendApi.Repositories.Interfaces;
using JatodaBackendApi.Services.CacheService.Interfaces;

namespace JatodaBackendApi.Services.CacheService;

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
}