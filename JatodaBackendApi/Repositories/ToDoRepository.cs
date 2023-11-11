using JatodaBackendApi.Model;
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
        await _context.Todonotes.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Todonote entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Todonote entity)
    {
        _context.Todonotes.Remove(entity);
        await _context.SaveChangesAsync();
    }
}