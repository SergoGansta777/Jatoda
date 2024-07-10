using Jatoda.Domain.Data.DBModels;

namespace Jatoda.Application.Interfaces;

public interface IToDoRepository
{
    void CreateTodo(Todo todo);
    void DeleteTodo(Todo todo);
    void UpdateTodo(Todo todo);
    Task<IEnumerable<Todo>> GetAllTodosAsync(bool trackChanges);
    Task<Todo?> GetTodoAsync(Guid todoId, bool trackChanges);
    Task<IEnumerable<Todo>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges);
    Task<IEnumerable<Todo>> GetByUserIdAsync(Guid userId, bool trackChanges);
    Task<IEnumerable<Todo>> GetCompletedByUserIdAsync(Guid userId, bool trackChanges);
}