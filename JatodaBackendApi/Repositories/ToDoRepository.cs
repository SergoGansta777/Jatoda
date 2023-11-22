using JatodaBackendApi.Models.DBModels;
using JatodaBackendApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JatodaBackendApi.Repositories;

public class ToDoRepository : IRepository<Todonote>
{
    private readonly JatodaContext _context;

    public ToDoRepository(JatodaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Todonote>> GetAllAsync()
    {
        return await _context.Todonotes.ToListAsync();
    }

    public async Task<Todonote?> GetByIdAsync(int id)
    {
        return await _context.Todonotes.FindAsync(id);
    }

    public async Task<Todonote> CreateAsync(Todonote entity)
    {
        var newTodo = await _context.Todonotes.AddAsync(entity);
        return newTodo.Entity;
    }

    public Task UpdateAsync(Todonote entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Todonote entity)
    {
        _context.Todonotes.Remove(entity);
        return Task.CompletedTask;
    }
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

}