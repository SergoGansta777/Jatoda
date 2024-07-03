using JatodaBackendApi.Models.DBModels;
using JatodaBackendApi.Providers.Interfaces;
using JatodaBackendApi.Repositories.Interfaces;
using JatodaBackendApi.Services.CacheService.Interfaces;

namespace JatodaBackendApi.Providers;

public class TodoProvider : ITodoProvider<Todonote>
{
    private static readonly TimeSpan DefaultTimeForCache = TimeSpan.FromMinutes(5);
    private readonly ICacheService _cacheService;
    private readonly ILogger<TodoProvider> _logger;
    private readonly IRepository<Todonote> _todoRepository;

    public TodoProvider(
        ICacheService cacheService,
        IRepository<Todonote> todoRepository,
        ILogger<TodoProvider> logger
    )
    {
        _cacheService = cacheService;
        _todoRepository = todoRepository;
        _logger = logger;
    }

    public async Task<List<Todonote>?> GetAllTodosAsync()
    {
        var todos = (await _todoRepository.GetAllAsync()).ToList();
        return todos;
    }

    public async Task<Todonote?> GetTodoByIdAsync(int id)
    {
        var cacheKey = $"todo:{id}";
        var todo = await _cacheService.GetFromCacheAsync<Todonote>(cacheKey);
        if (todo != null) return todo;
        try
        {
            todo = await _todoRepository.GetByIdAsync(id);
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

    public async Task<Todonote> AddTodoAsync(Todonote todo)
    {
        todo.Createdat = DateTime.Now.ToUniversalTime();
        todo.Updatedat = DateTime.Now.ToUniversalTime();

        var createdTodo = await _todoRepository.CreateAsync(todo);
        await _cacheService.SetCacheAsync(
            $"todo:{createdTodo.Id}",
            createdTodo,
            DefaultTimeForCache
        );
        _logger.LogInformation("Added new todo with id {createdId} and set it in the cache", createdTodo.Id);

        return createdTodo;
    }

    public async Task UpdateTodoAsync(Todonote todo)
    {
        todo.Updatedat = DateTime.Now.ToUniversalTime();

        await _todoRepository.UpdateAsync(todo);
        await _cacheService.RemoveFromCacheAsync($"todo:{todo.Id}");
        _logger.LogInformation("Updated todo with id {id} and removed it from the cache", todo.Id);
    }

    public async Task DeleteTodoAsync(Todonote todo)
    {
        await _todoRepository.DeleteAsync(todo);
        await _cacheService.RemoveFromCacheAsync($"todo:{todo.Id}");
        _logger.LogInformation("Deleted todo with id {id} and removed it from the cache", todo.Id);
    }

    public async Task<List<Todonote>?> GetTodosByUserIdAsync(int userId)
    {
        var todos = (await _todoRepository.GetAllAsync()).Where(t => t.Userid == userId).ToList();
        return todos;
    }

    public async Task<List<Todonote>?> GetCompletedTodosByUserIdAsync(int userId)
    {
        var todos = (await _todoRepository.GetAllAsync()).Where(t => t.Userid == userId && t.CompletedOn != null)
            .ToList();
        return todos;
    }

    public async Task<List<Todonote>?> GetTodosWithDifficultyLevelAsync(int difficultyLevel)
    {
        return await Task.FromResult(
            _todoRepository
                .GetAllAsync()
                .Result.Where(t => t.Difficultylevel == difficultyLevel)
                .ToList()
        );
    }

    public async Task<List<Todonote>?> GetTodosWithTagAsync(int tagId)
    {
        return await Task.FromResult(
            _todoRepository
                .GetAllAsync()
                .Result.Where(todo => todo.Tags.Any(tag => tag.Id == tagId))
                .ToList()
        );
    }
}