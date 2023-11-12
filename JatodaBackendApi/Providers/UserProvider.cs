using JatodaBackendApi.Model;
using JatodaBackendApi.Providers.Interfaces;
using JatodaBackendApi.Repositories.Interfaces;

namespace JatodaBackendApi.Providers;

public class UserProvider : IUserProvider<User>
{
    private static readonly TimeSpan defaultTimeForCache = TimeSpan.FromMinutes(5);
    private readonly ICacheRepository _cache;
    private readonly IRepository<User> _userRepository;

    public UserProvider(ICacheRepository cache, IRepository<User> userRepository)
    {
        _cache = cache;
        _userRepository = userRepository;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        var cacheKey = $"user:{username}";
        var isUserCached = await _cache.IsExistsInCacheAsync(cacheKey);
        if (isUserCached) return await _cache.GetFromCacheAsync<User>(cacheKey);

        var users = await _userRepository.GetAllAsync();
        var user = users.FirstOrDefault(u => u.Username == username);
        if (user != null) await _cache.SetCacheAsync(cacheKey, user, defaultTimeForCache);

        return user;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var cacheKey = $"user_email:{email}";
        var isUserCached = await _cache.IsExistsInCacheAsync(cacheKey);
        if (isUserCached) return await _cache.GetFromCacheAsync<User>(cacheKey);

        var users = await _userRepository.GetAllAsync();
        var user = users.FirstOrDefault(u => u.Email == email);
        if (user != null) await _cache.SetCacheAsync(cacheKey, user, defaultTimeForCache);

        return user;
    }

    public async Task<User> AddUserAsync(User user)
    {
        return await _userRepository.CreateAsync(user);
    }
}