using JatodaBackendApi.Models.DBModels;

namespace JatodaBackendApi.Providers.Interfaces;

public interface ITodoProvider<T> where T : class
{
    Task<List<Todo>?> GetAllTodosAsync();
    Task<Todo?> GetTodoByIdAsync(int id);
    Task<T> AddTodoAsync(T entity);
    Task UpdateTodoAsync(T entity);
    Task DeleteTodoAsync(T entity);
    Task<List<T>?> GetTodosByUserIdAsync(int id);
    Task<List<T>?> GetTodosWithTagAsync(int tagId);
    Task<List<T>?> GetTodosWithDifficultyLevelAsync(int difficultyLevel);
    Task<List<Todo>?> GetCompletedTodosByUserIdAsync(int userId);
}