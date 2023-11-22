using JatodaBackendApi.Models.DBModels;
using JatodaBackendApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JatodaBackendApi.Repositories;

public class UserRepository : IRepository<User>
{
    private readonly JatodaContext _context;

    public UserRepository(JatodaContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return (await _context.Users.FindAsync(id))!;
    }

    public async Task<User> CreateAsync(User user)
    {
        var newUser = _context.Users.Add(user);
        return newUser.Entity;
    }

    public Task UpdateAsync(User user)
    {
        _context.Entry(user).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User user)
    {
        _context.Users.Remove(user);
        return Task.CompletedTask;
    }
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

}