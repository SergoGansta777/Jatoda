using Jatoda.Application.Interfaces;
using Jatoda.Domain.Core.DBModels;
using Jatoda.Infrastructure.CacheService.Interfaces;
using static System.TimeSpan;

namespace Jatoda.Infrastructure.EFCore.Repositories;

public class CachingToDoRepository(JatodaContext context, IToDoRepository repository, ICacheService cacheService)
    : RepositoryBase<Todo>(context), IToDoRepository
{
    private static readonly TimeSpan DefaultTimeForCache = FromMinutes(3);

    public async void CreateTodo(Todo todo)
    {
        repository.CreateTodo(todo);
        await InvalidateCacheForUser(todo.UserId);
    }

    public async void DeleteTodo(Todo todo)
    {
        repository.DeleteTodo(todo);
        await InvalidateCacheForUser(todo.UserId);
    }

    public async void UpdateTodo(Todo todo)
    {
        repository.UpdateTodo(todo);
        await InvalidateCacheForUser(todo.UserId);
    }

    public async Task<IEnumerable<Todo>> GetAllTodosAsync(bool trackChanges)
    {
        return await repository.GetAllTodosAsync(trackChanges);
    }

    public async Task<Todo?> GetTodoAsync(Guid todoId, bool trackChanges)
    {
        return await repository.GetTodoAsync(todoId, trackChanges);
    }

    public async Task<IEnumerable<Todo>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges)
    {
        return await repository.GetByIdsAsync(ids, trackChanges);
    }

    public async Task<IEnumerable<Todo>> GetByUserIdAsync(Guid userId, bool trackChanges)
    {
        return await cacheService.GetOrCreateCacheAsync(
            $"{userId}-todos",
            async () => await repository.GetByUserIdAsync(userId, trackChanges),
            DefaultTimeForCache);
    }

    public async Task<IEnumerable<Todo>> GetCompletedByUserIdAsync(Guid userId, bool trackChanges)
    {
        return await cacheService.GetOrCreateCacheAsync(
            $"{userId}-completed",
            async () => await repository.GetCompletedByUserIdAsync(userId, trackChanges),
            DefaultTimeForCache);
    }

    private async Task InvalidateCacheForUser(Guid userId)
    {
        await cacheService.RemoveFromCacheAsync($"{userId}-todos");
        await cacheService.RemoveFromCacheAsync($"{userId}-completed");
    }
}