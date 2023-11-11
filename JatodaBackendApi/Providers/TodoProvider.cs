using JatodaBackendApi.Model;
using JatodaBackendApi.Providers.Interfaces;
using JatodaBackendApi.Repositories.Interfaces;

namespace JatodaBackendApi.Providers;

public class TodoProvider : ITodoProvider<Todonote>
{
    private static readonly TimeSpan defaultTimeForCache = TimeSpan.FromMinutes(5);
    private readonly ICacheRepository _cache;
    private readonly IRepository<Todonote> _todoRepository;

    public TodoProvider(ICacheRepository cache, IRepository<Todonote> todoRepository)
    {
        _cache = cache;
        _todoRepository = todoRepository;
    }

    public async Task<List<Todonote>?> GetAllTodosAsync()
    {
        return await _todoRepository.GetAllAsync() as List<Todonote>;
    }

    public async Task<Todonote?> GetTodoByIdAsync(int id)
    {
        var cacheKey = $"todo:{id}";
        if (await _cache.IsExistsInCacheAsync(cacheKey)) return await _cache.GetFromCacheAsync<Todonote>(cacheKey);

        var todo = await _todoRepository.GetByIdAsync(id);
        if (todo != null) await _cache.SetCacheAsync(cacheKey, todo, defaultTimeForCache);

        return todo;
    }

    public async Task<Todonote> AddTodoAsync(Todonote todo)
    {
        todo.Createdat = DateTime.Now;
        todo.Updatedat = DateTime.Now;

        var createdTodo = await _todoRepository.CreateAsync(todo);
        await _cache.SetCacheAsync($"todo:{createdTodo.Id}", createdTodo, defaultTimeForCache);

        return createdTodo;
    }

    public async Task UpdateTodoAsync(Todonote todo)
    {
        todo.Updatedat = DateTime.Now;

        await _todoRepository.DeleteAsync(todo);
        await _cache.RemoveFromCacheAsync($"todo:{todo.Id}");
    }

    public async Task DeleteTodoAsync(Todonote todo)
    {
        await _todoRepository.DeleteAsync(todo);
        await _cache.RemoveFromCacheAsync($"todo:{todo.Id}");
    }

    public async Task<List<Todonote>> GetTodosByUserIdAsync(int userId)
    {
        var cacheKey = $"todos:{userId}";
        if (await _cache.IsExistsInCacheAsync(cacheKey))
            return await _cache.GetFromCacheAsync<List<Todonote>>(cacheKey) ?? new List<Todonote>();

        var todos = await _todoRepository.GetAllAsync();
        var userTodos = todos.Where(t => t.Userid == userId).ToList();
        await _cache.SetCacheAsync(cacheKey, userTodos, defaultTimeForCache);

        return userTodos;
    }

    public async Task<List<Todonote>> GetTodosWithDifficultyLevelAsync(int difficultyLevel)
    {
        var todos = await _todoRepository.GetAllAsync();
        var todosWithDifficultyLevel = todos.Where(t => t.Difficultylevel == difficultyLevel).ToList();

        return todosWithDifficultyLevel;
    }

    public async Task<List<Todonote>> GetTodosWithTagAsync(int tagId)
    {
        var todos = await _todoRepository.GetAllAsync();
        var todosWithTag = todos.Where(t => t.Tags.Any(t => t.Id == tagId)).ToList();

        return todosWithTag;
    }
}