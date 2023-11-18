using JatodaBackendApi.Services.CacheService.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace JatodaBackendApi.Services.CacheService.Repositories;

public class CacheRepository : ICacheRepository
{
    private readonly IDistributedCache _cache;

    public CacheRepository(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetFromCacheAsync<T>(string key)
    {
        var serializedValue = await _cache.GetStringAsync(key);
        return serializedValue is not null
            ? JsonConvert.DeserializeObject<T>(serializedValue)
            : default;
    }

    public async Task SetCacheAsync<T>(string key, T value, TimeSpan expiration)
    {
        var serializedValue = JsonConvert.SerializeObject(value);
        var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(expiration);
        await _cache.SetStringAsync(key, serializedValue, options);
    }

    public async Task RemoveFromCacheAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }
}