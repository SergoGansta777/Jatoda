using Jatoda.Application.Interfaces;
using Jatoda.Domain.Core.DBModels;
using Jatoda.Infrastructure.CacheService.Interfaces;

namespace Jatoda.Infrastructure.DataEFCore.Repositories;

public class CachingUserRepository(IUserRepository repository, ICacheService cacheService) : IUserRepository
{
    private static readonly TimeSpan DefaultTimeForCache = TimeSpan.FromMinutes(3);

    public async void CreateUser(User user)
    {
        repository.CreateUser(user);
        await InvalidateCacheForUser(user.Id, user.Username, user.Email);
    }

    public async void DeleteUser(User user)
    {
        repository.DeleteUser(user);
        await InvalidateCacheForUser(user.Id, user.Username, user.Email);
    }

    public async void UpdateUser(User user)
    {
        repository.UpdateUser(user);
        await InvalidateCacheForUser(user.Id, user.Username, user.Email);
    }

    public async Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges)
    {
        return await repository.GetByIdsAsync(ids, trackChanges);
    }

    public async Task<User?> GetByIdAsync(Guid id, bool trackChanges)
    {
        return await cacheService.GetOrCreateCacheAsync(
            $"{id}-user",
            async () => await repository.GetByIdAsync(id, trackChanges),
            DefaultTimeForCache);
    }

    public async Task<User?> GetByUsernameAsync(string username, bool trackChanges)
    {
        return await cacheService.GetOrCreateCacheAsync(
            $"{username}-user",
            async () => await repository.GetByUsernameAsync(username, trackChanges),
            DefaultTimeForCache);
    }

    public async Task<User?> GetByEmailAsync(string email, bool trackChanges)
    {
        return await cacheService.GetOrCreateCacheAsync(
            $"{email}-user",
            async () => await repository.GetByEmailAsync(email, trackChanges),
            DefaultTimeForCache);
    }

    private async Task InvalidateCacheForUser(Guid id, string username, string email)
    {
        await cacheService.RemoveFromCacheAsync($"{id}-user");
        await cacheService.RemoveFromCacheAsync($"{username}-user");
        await cacheService.RemoveFromCacheAsync($"{email}-user");
    }
}