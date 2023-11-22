using JatodaBackendApi.Models.DBModels;
using JatodaBackendApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JatodaBackendApi.Repositories;

public class TagRepository : IRepository<Tag>
{
    private readonly JatodaContext _context;

    public TagRepository(JatodaContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Tag>> GetAllAsync()
    {
        return await _context.Tags.ToListAsync();
    }

    public async Task<Tag?> GetByIdAsync(int id)
    {
        return await _context.Tags.FindAsync(id);
    }

    public Task<Tag> CreateAsync(Tag tag)
    {
        var newTag = _context.Tags.Add(tag);
        return Task.FromResult(newTag.Entity);
    }

    public Task UpdateAsync(Tag tag)
    {
        _context.Entry(tag).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Tag tag)
    {
        _context.Tags.Remove(tag);
        return Task.CompletedTask;
    }
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}