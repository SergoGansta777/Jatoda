using JatodaBackendApi.Models.DBModels;
using JatodaBackendApi.Providers.Interfaces;
using JatodaBackendApi.Repositories;
using JatodaBackendApi.Repositories.Interfaces;
using JatodaBackendApi.Services.CacheService.Interfaces;

namespace JatodaBackendApi.Providers;

public class TodoProvider : ITodoProvider<Todo>
{
    private static readonly TimeSpan DefaultTimeForCache = TimeSpan.FromMinutes(5);
    private readonly ICacheService _cacheService;
    private readonly ILogger<TodoProvider> _logger;
    private readonly IToDoRepository _todoRepository;

    public TodoProvider(
        ICacheService cacheService,
        ILogger<TodoProvider> logger, IRepositoryManager repository)
    {
        _cacheService = cacheService;
        _logger = logger;
        _todoRepository = repository.Todo;
    }

    public async Task<List<Todo>?> GetAllTodosAsync()
    {
        var todos = (await _todoRepository.GetAllTodosAsync(true)).ToList();
        return todos;
    }

    public async Task<Todo?> GetTodoByIdAsync(Guid id)
    {
        var cacheKey = $"todo:{id}";
        var todo = await _cacheService.GetFromCacheAsync<Todo>(cacheKey);
        if (todo is not null)
        {
            return todo;
        }

        try
        {
            todo = await _todoRepository.GetTodoAsync(id, false);
            await _cacheService.SetCacheAsync(cacheKey, todo, DefaultTimeForCache);
            _logger.LogInformation(
                "Retrieved todo with id {id} from the repository and set it in the cache", id
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving todo with id {id} from the repository", id);
        }

        return todo;
    }

    public async Task<Todo> AddTodoAsync(Todo todo)
    {
        todo.CreateDate = DateTime.Now.ToUniversalTime();
        todo.UpdateDate = DateTime.Now.ToUniversalTime();

        _todoRepository.CreateTodo(todo);
        await _cacheService.SetCacheAsync(
            $"todo:{todo.Id}",
            todo,
            DefaultTimeForCache
        );
        _logger.LogInformation("Added new todo with id {createdId} and set it in the cache", todo.Id);

        return todo;
    }

    public async Task UpdateTodoAsync(Todo todo)
    {
        todo.UpdateDate = DateTime.Now.ToUniversalTime();

        _todoRepository.UpdateTodo(todo);
        await _cacheService.RemoveFromCacheAsync($"todo:{todo.Id}");
        _logger.LogInformation("Updated todo with id {id} and removed it from the cache", todo.Id);
    }

    public async Task DeleteTodoAsync(Todo todo)
    {
        _todoRepository.DeleteTodo(todo);
        await _cacheService.RemoveFromCacheAsync($"todo:{todo.Id}");
        _logger.LogInformation("Deleted todo with id {id} and removed it from the cache", todo.Id);
    }

    public async Task<List<Todo>> GetTodosByUserIdAsync(Guid userId)
    {
        var todos = (await _todoRepository.GetByUserIdAsync(userId, false)).ToList();
        _logger.LogInformation("Got {count} todos for user with userId {userId}", todos.Count, userId);
        return todos.ToList();
    }

    public async Task<List<Todo>> GetCompletedTodosByUserIdAsync(Guid userId)
    {
        var todos = (await _todoRepository.GetCompletedByUserIdAsync(userId, false)).ToList();
        _logger.LogInformation("Got {count} completed todos for user with userId {userId}", todos.Count, userId);
        return todos;
    }
}