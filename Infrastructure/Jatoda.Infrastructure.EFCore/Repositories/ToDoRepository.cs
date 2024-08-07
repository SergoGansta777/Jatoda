using Jatoda.Application.Interfaces;
using Jatoda.Domain.Data.DBModels;
using Microsoft.EntityFrameworkCore;

namespace Jatoda.Infrastructure.EFCore.Repositories;

public class ToDoRepository(JatodaContext context) : RepositoryBase<Todo>(context), IToDoRepository
{
    public void CreateTodo(Todo todo)
    {
        Create(todo);
    }

    public void DeleteTodo(Todo todo)
    {
        Delete(todo);
    }

    public void UpdateTodo(Todo todo)
    {
        Update(todo);
    }

    public async Task<IEnumerable<Todo>> GetAllTodosAsync(bool trackChanges)
    {
        return await FindAll(trackChanges)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Todo?> GetTodoAsync(Guid todoId, bool trackChanges)
    {
        return await FindByCondition(c => c.Id.Equals(todoId), trackChanges)
            .SingleOrDefaultAsync();
    }

    public async Task<IEnumerable<Todo>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges)
    {
        return await FindByCondition(x => ids.Contains(x.Id), trackChanges)
            .ToListAsync();
    }

    public async Task<IEnumerable<Todo>> GetByUserIdAsync(Guid userId, bool trackChanges)
    {
        return await FindByCondition(x => x.UserId == userId, trackChanges)
            .ToListAsync();
    }

    public async Task<IEnumerable<Todo>> GetCompletedByUserIdAsync(Guid userId, bool trackChanges)
    {
        return await FindByCondition(x => x.UserId == userId && x.CompletedOn != null, trackChanges)
            .ToListAsync();
    }
}