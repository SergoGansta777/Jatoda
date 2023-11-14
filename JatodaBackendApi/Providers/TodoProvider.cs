using JatodaBackendApi.Model;
using JatodaBackendApi.Providers.Interfaces;
using JatodaBackendApi.Services.CacheService.Interfaces;
using JatodaBackendApi.Repositories.Interfaces;

namespace JatodaBackendApi.Providers
{
    public class TodoProvider : ITodoProvider<Todonote>
    {
        private static readonly TimeSpan DefaultTimeForCache = TimeSpan.FromMinutes(5);
        private readonly ICacheService _cacheService;
        private readonly IRepository<Todonote> _todoRepository;
        private readonly ILogger<TodoProvider> _logger;

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
            var todos = await _cacheService.GetFromCacheAsync<List<Todonote>>("todos");
            if (todos == null)
            {
                try
                {
                    todos = (await _todoRepository.GetAllAsync()).ToList();
                    await _cacheService.SetCacheAsync("todos", todos, DefaultTimeForCache);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving todos from the repository");
                }
            }
            return todos;
        }

        public async Task<Todonote?> GetTodoByIdAsync(int id)
        {
            var cacheKey = $"todo:{id}";
            var todo = await _cacheService.GetFromCacheAsync<Todonote>(cacheKey);
            if (todo == null)
            {
                try
                {
                    todo = await _todoRepository.GetByIdAsync(id);
                    await _cacheService.SetCacheAsync(cacheKey, todo, DefaultTimeForCache);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error retrieving todo with id {id} from the repository");
                }
            }
            return todo;
        }

        public async Task<Todonote> AddTodoAsync(Todonote todo)
        {
            todo.Createdat = DateTime.Now;
            todo.Updatedat = DateTime.Now;

            var createdTodo = await _todoRepository.CreateAsync(todo);
            await _cacheService.SetCacheAsync(
                $"todo:{createdTodo.Id}",
                createdTodo,
                DefaultTimeForCache
            );

            return createdTodo;
        }

        public async Task UpdateTodoAsync(Todonote todo)
        {
            todo.Updatedat = DateTime.Now;

            await _todoRepository.UpdateAsync(todo);
            await _cacheService.RemoveFromCacheAsync($"todo:{todo.Id}");
        }

        public async Task DeleteTodoAsync(Todonote todo)
        {
            await _todoRepository.DeleteAsync(todo);
            await _cacheService.RemoveFromCacheAsync($"todo:{todo.Id}");
        }

        public async Task<List<Todonote>?> GetTodosByUserIdAsync(int userId)
        {
            var cacheKey = $"todos:{userId}";
            var todos = await _cacheService.GetFromCacheAsync<List<Todonote>>(cacheKey);
            if (todos == null)
            {
                try
                {
                    todos = _todoRepository
                        .GetAllAsync()
                        .Result.Where(t => t.Userid == userId)
                        .ToList();
                    await _cacheService.SetCacheAsync(cacheKey, todos, DefaultTimeForCache);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        $"Error retrieving todos for user with id {userId} from the repository"
                    );
                }
            }
            return todos;
        }

        public async Task<List<Todonote>> GetTodosWithDifficultyLevelAsync(int difficultyLevel)
        {
            return await Task.FromResult(
                _todoRepository
                    .GetAllAsync()
                    .Result.Where(t => t.Difficultylevel == difficultyLevel)
                    .ToList()
            );
        }

        public async Task<List<Todonote>> GetTodosWithTagAsync(int tagId)
        {
            return await Task.FromResult(
                _todoRepository
                    .GetAllAsync()
                    .Result.Where(todo => todo.Tags.Any(tag => tag.Id == tagId))
                    .ToList()
            );
        }
    }
}
