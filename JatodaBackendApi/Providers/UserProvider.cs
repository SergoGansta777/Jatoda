using JatodaBackendApi.Models.DBModels;
using JatodaBackendApi.Providers.Interfaces;
using JatodaBackendApi.Repositories;
using JatodaBackendApi.Repositories.Interfaces;
using JatodaBackendApi.Services.CacheService.Interfaces;
using static System.DateTime;

namespace JatodaBackendApi.Providers;

public class UserProvider : IUserProvider<User>
{
    private static readonly TimeSpan DefaultTimeForCache = TimeSpan.FromMinutes(5);
    private readonly ICacheService _cacheService;
    private readonly IUserRepository _userRepository;

    public UserProvider(ICacheService cacheService, IRepositoryManager repositoryManager)
    {
        _cacheService = cacheService;
        _userRepository = repositoryManager.User;
    }

    public async Task<User?> GetByUsernameAsync(string? username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return null;
        }

        var cacheKey = $"user:{username}";
        var user = await _cacheService.GetFromCacheAsync<User>(cacheKey);

        if (user is not null)
        {
            return user;
        }

        user = await _userRepository.GetByUsernameAsync(username, false);

        if (user is not null)
        {
            await _cacheService.SetCacheAsync(cacheKey, user, DefaultTimeForCache);
        }

        return user;
    }

    public async Task<User?> GetByEmailAsync(string? email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return null;
        }

        var cacheKey = $"user_email:{email}";
        var user = await _cacheService.GetFromCacheAsync<User>(cacheKey);

        if (user is not null)
        {
            return user;
        }

        user = await _userRepository.GetByEmailAsync(email, false);
        if (user is not null)
        {
            await _cacheService.SetCacheAsync(cacheKey, user, DefaultTimeForCache);
        }

        return user;
    }

    public async Task<User> AddUserAsync(User user)
    {
        user.CreateDate = Now.ToUniversalTime();
        user.UpdateDate = Now.ToUniversalTime();

        _userRepository.CreateUser(user);
        await _cacheService.SetCacheAsync(
            $"user:{user.Id}",
            user,
            DefaultTimeForCache
        );

        return user;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        var cacheKey = $"user_id:{id}";
        var user = await _cacheService.GetFromCacheAsync<User>(cacheKey);

        if (user is not null)
        {
            return user;
        }

        user = await _userRepository.GetByIdAsync(id, false);
        if (user is not null)
        {
            await _cacheService.SetCacheAsync(cacheKey, user, DefaultTimeForCache);
        }

        return user;
    }
}