using JatodaBackendApi.Models.DBModels;

namespace JatodaBackendApi.Repositories;

public interface IToDoRepository
{
    Task<IEnumerable<Todo>> GetAllTodosAsync(bool trackChanges);
    Task<Todo?> GetTodoAsync(Guid todoId, bool trackChanges);
    void CreateTodo(Todo todo);
    Task<IEnumerable<Todo>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges);
    void DeleteTodo(Todo todo);
    void UpdateTodo(Todo todo);
}