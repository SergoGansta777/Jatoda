using JatodaBackendApi.Providers.Interfaces;
using JatodaBackendApi.Repositories.Interfaces;
using JatodaBackendApi.Model;

namespace JatodaBackendApi.Providers;

public class UserProvider: IUserProvider<User>
{
    private readonly ICacheRepository _cache;
    private readonly IRepository<User> _userRepository;
    private static readonly TimeSpan defaultTimeForCache = TimeSpan.FromMinutes(5);

    public UserProvider(ICacheRepository cache, IRepository<User> userRepository)
    {
        _cache = cache;
        _userRepository = userRepository;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        var isUserCached = await _cache.IsExistsInCacheAsync(username);
        if (isUserCached)
        {
            return await _cache.GetFromCacheAsync<User>(username);
        }

        var users = await _userRepository.GetAllAsync();
        var user = users.FirstOrDefault(u => u.Username == username);
        if (user != null)
        {
            await _cache.SetCacheAsync(username, user, defaultTimeForCache);
        }

        return user;
    }

    public async Task<User> AddUserAsync(User user)
    {
        return await _userRepository.CreateAsync(user);
    }
}