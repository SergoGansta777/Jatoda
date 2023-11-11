using JatodaBackendApi.Repositories.Interfaces;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace JatodaBackendApi.Repositories;

public class CacheRepository : ICacheRepository
{
    private readonly IDatabase _database;

    public CacheRepository(IConnectionMultiplexer redisConnection)
    {
        _database = redisConnection.GetDatabase();
    }

    public async Task<T> GetFromCacheAsync<T>(string key)
    {
        var serializedValue = await _database.StringGetAsync(key);
        if (serializedValue.HasValue) return Deserialize<T>(serializedValue);

        return default;
    }

    public async Task SetCacheAsync<T>(string key, T value, TimeSpan expiration)
    {
        var serializedValue = Serialize(value);
        await _database.StringSetAsync(key, serializedValue, expiration);
    }

    public async Task RemoveFromCacheAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }

    public async Task<bool> IsExistsInCacheAsync(string key)
    {
        return await _database.KeyExistsAsync(key);
    }

    private T Deserialize<T>(RedisValue serializedValue)
    {
        return JsonConvert.DeserializeObject<T>(serializedValue);
    }

    private RedisValue Serialize<T>(T value)
    {
        return JsonConvert.SerializeObject(value);
    }
}