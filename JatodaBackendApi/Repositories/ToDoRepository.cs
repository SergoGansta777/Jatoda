using JatodaBackendApi.Factories;
using JatodaBackendApi.Models.DBModels;
using Microsoft.EntityFrameworkCore;

namespace JatodaBackendApi.Repositories;

public class ToDoRepository(JatodaContext context) : RepositoryBase<Todo>(context), IToDoRepository
{
    private readonly JatodaContext _context = context ?? throw new ArgumentNullException(nameof(context));
    
    public async Task<IEnumerable<Todo>> GetAllTodosAsync(bool trackChanges) =>
        await FindAll(trackChanges)
            .OrderBy(c => c.Name)
            .ToListAsync();

    public async Task<Todo?> GetTodoAsync(Guid todoId, bool trackChanges) =>
        await FindByCondition(c => c.Id.Equals(todoId), trackChanges)
            .SingleOrDefaultAsync();

    public void CreateTodo(Todo todo) => Create(todo);

    public async Task<IEnumerable<Todo>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges) =>
        await FindByCondition(x => ids.Contains(x.Id), trackChanges)
            .ToListAsync();
    
    public async Task<IEnumerable<Todo>> GetByUserIdAsync(Guid userId, bool trackChanges) =>
        await FindByCondition(x => x.UserId == userId, trackChanges)
            .ToListAsync();

    public void DeleteTodo(Todo todo) => Delete(todo);
    public void UpdateTodo(Todo todo) => Update(todo);
}