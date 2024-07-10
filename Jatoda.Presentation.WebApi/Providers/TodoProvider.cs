using Jatoda.Application.Interfaces;
using Jatoda.Domain.Data.DBModels;
using Jatoda.Providers.Interfaces;
using static System.DateTime;

namespace Jatoda.Providers;

public class TodoProvider(ILogger<TodoProvider> logger, IRepositoryManager repository)
    : ITodoProvider<Todo>
{
    public async Task<List<Todo>?> GetAllTodosAsync()
    {
        var todos = (await repository.Todo.GetAllTodosAsync(true)).ToList();
        return todos;
    }

    public async Task<Todo?> GetTodoByIdAsync(Guid id)
    {
        var todo = await repository.Todo.GetTodoAsync(id, false);
        logger.LogInformation(
            "Retrieved todo with id {id} from the repository and set it in the cache", id
        );

        return todo;
    }

    public Task<Todo> AddTodoAsync(Todo todo)
    {
        todo.CreateDate = Now.ToUniversalTime();
        todo.UpdateDate = Now.ToUniversalTime();

        repository.Todo.CreateTodo(todo);
        repository.Save();

        logger.LogInformation("Added new todo with id {createdId} and set it in the cache", todo.Id);

        return Task.FromResult(todo);
    }

    public Task UpdateTodoAsync(Todo todo)
    {
        todo.UpdateDate = Now.ToUniversalTime();

        repository.Todo.UpdateTodo(todo);
        repository.Save();

        logger.LogInformation("Updated todo with id {id} and removed it from the cache", todo.Id);
        return Task.CompletedTask;
    }

    public Task DeleteTodoAsync(Todo todo)
    {
        repository.Todo.DeleteTodo(todo);
        repository.Save();
        logger.LogInformation("Deleted todo with id {id} and removed it from the cache", todo.Id);
        return Task.CompletedTask;
    }

    public async Task<List<Todo>> GetTodosByUserIdAsync(Guid userId)
    {
        var todos = (await repository.Todo.GetByUserIdAsync(userId, false)).ToList();
        logger.LogInformation("Got {count} todos for user with userId {userId}", todos.Count, userId);
        return todos.ToList();
    }

    public async Task<List<Todo>> GetCompletedTodosByUserIdAsync(Guid userId)
    {
        var todos = (await repository.Todo.GetCompletedByUserIdAsync(userId, false)).ToList();
        logger.LogInformation("Got {count} completed todos for user with userId {userId}", todos.Count, userId);
        return todos;
    }
}