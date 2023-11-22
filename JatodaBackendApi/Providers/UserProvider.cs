using JatodaBackendApi.Models;
using JatodaBackendApi.Models.DBModels;
using JatodaBackendApi.Providers.Interfaces;
using JatodaBackendApi.Repositories.Interfaces;
using JatodaBackendApi.Services.CacheService.Interfaces;

namespace JatodaBackendApi.Providers;

public class UserProvider : IUserProvider<User>
{
    private static readonly TimeSpan DefaultTimeForCache = TimeSpan.FromMinutes(5);
    private readonly ICacheService _cacheService;
    private readonly IRepository<User> _userRepository;

    public UserProvider(ICacheService cacheService, IRepository<User> userRepository)
    {
        _cacheService = cacheService;
        _userRepository = userRepository;
    }

    public async Task<User?> GetByUsernameAsync(string? username)
    {
        var cacheKey = $"user:{username}";
        var user = await _cacheService.GetFromCacheAsync<User>(cacheKey);

        if (user != null) return user;
        user = (await _userRepository.GetAllAsync()).FirstOrDefault(
            u => u.Username == username
        );
        if (user != null) await _cacheService.SetCacheAsync(cacheKey, user, DefaultTimeForCache);

        return user;
    }

    public async Task<User?> GetByEmailAsync(string? email)
    {
        var cacheKey = $"user_email:{email}";
        var user = await _cacheService.GetFromCacheAsync<User>(cacheKey);

        if (user != null) return user;
        user = (await _userRepository.GetAllAsync()).FirstOrDefault(u => u.Email == email);
        if (user != null) await _cacheService.SetCacheAsync(cacheKey, user, DefaultTimeForCache);

        return user;
    }

    public async Task<User> AddUserAsync(User user)
    {
        user.Createdat = DateTime.Now.ToUniversalTime();
        user.Updatedat = DateTime.Now.ToUniversalTime();
        
        await _userRepository.CreateAsync(user);
        return user;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        var cacheKey = $"user_id:{id}";
        var user = await _cacheService.GetFromCacheAsync<User>(cacheKey);

        if (user != null) return user;
        user = await _userRepository.GetByIdAsync(id);
        if (user != null) await _cacheService.SetCacheAsync(cacheKey, user, DefaultTimeForCache);

        return user;
    }
}