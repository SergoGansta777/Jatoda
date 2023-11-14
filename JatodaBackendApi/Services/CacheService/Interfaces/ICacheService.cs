namespace JatodaBackendApi.Services.CacheService.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetFromCacheAsync<T>(string key);
        Task SetCacheAsync<T>(string key, T value, TimeSpan expiration);
        Task RemoveFromCacheAsync(string key);
    }
}
