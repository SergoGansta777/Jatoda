namespace JatodaBackendApi.Repositories.Interfaces;

public interface ICacheRepository
{
    Task<T> GetFromCacheAsync<T>(string key);
    Task SetCacheAsync<T>(string key, T value, TimeSpan expiration);
    Task RemoveFromCacheAsync(string key);
    Task<bool> IsExistsInCacheAsync(string key);
}