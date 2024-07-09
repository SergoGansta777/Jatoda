using JatodaBackendApi.Models.DBModels;

namespace JatodaBackendApi.Providers.Interfaces;

public interface ITodoProvider<T> where T : class
{
    Task<List<Todo>?> GetAllTodosAsync();
    Task<Todo?> GetTodoByIdAsync(Guid id);
    Task<T> AddTodoAsync(T entity);
    Task UpdateTodoAsync(T entity);
    Task DeleteTodoAsync(T entity);
    Task<List<Todo>> GetTodosByUserIdAsync(Guid id);
    Task<List<Todo>> GetCompletedTodosByUserIdAsync(Guid userId);
}