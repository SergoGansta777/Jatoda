using JatodaBackendApi.Models.DBModels;
using JatodaBackendApi.Providers.Interfaces;
using JatodaBackendApi.Repositories.Interfaces;
using JatodaBackendApi.Services.CacheService.Interfaces;

namespace JatodaBackendApi.Providers;

public class TodoProvider : ITodoProvider<Todo>
{
    private static readonly TimeSpan DefaultTimeForCache = TimeSpan.FromMinutes(5);
    private readonly ICacheService _cacheService;
    private readonly ILogger<TodoProvider> _logger;
    private readonly IRepositoryManager _repository;

    public TodoProvider(
        ICacheService cacheService,
        ILogger<TodoProvider> logger, IRepositoryManager repository)
    {
        _cacheService = cacheService;
        _logger = logger;
        _repository = repository;
    }

    public async Task<List<Todo>?> GetAllTodosAsync()
    {
        var todos = (await _repository.Todo.GetAllTodosAsync(true)).ToList();
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
            todo = await _repository.Todo.GetTodoAsync(id, false);
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

        _repository.Todo.CreateTodo(todo);
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

        _repository.Todo.UpdateTodo(todo);
        await _cacheService.RemoveFromCacheAsync($"todo:{todo.Id}");
        _logger.LogInformation("Updated todo with id {id} and removed it from the cache", todo.Id);
    }

    public async Task DeleteTodoAsync(Todo todo)
    {
        _repository.Todo.DeleteTodo(todo);
        await _cacheService.RemoveFromCacheAsync($"todo:{todo.Id}");
        _logger.LogInformation("Deleted todo with id {id} and removed it from the cache", todo.Id);
    }

    public async Task<List<Todo>?> GetTodosByUserIdAsync(int userId)
    {
        var todos = (await _repository.Todo.GetAllTodosAsync(false)).Where(t => t. == userId).ToList();
        return todos;
    }

    public async Task<List<Todo>?> GetCompletedTodosByUserIdAsync(int userId)
    {
        var todos = (await _todoRepository.GetAllAsync()).Where(t => t.Userid == userId && t.CompletedOn is not null)
            .ToList();
        return todos;
    }

    public async Task<List<Todo>?> GetTodosWithDifficultyLevelAsync(int difficultyLevel)
    {
        return await Task.FromResult(
            _todoRepository
                .GetAllAsync()
                .Result.Where(t => t.Difficultylevel == difficultyLevel)
                .ToList()
        );
    }

    public async Task<List<Todo>?> GetTodosWithTagAsync(int tagId)
    {
        return await Task.FromResult(
            _todoRepository
                .GetAllAsync()
                .Result.Where(todo => todo.Tags.Any(tag => tag.Id == tagId))
                .ToList()
        );
    }
}