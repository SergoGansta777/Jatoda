using Jatoda.Application.Interfaces;
using Jatoda.Domain.Data.DBModels;
using Microsoft.EntityFrameworkCore;

namespace Jatoda.Infrastructure.DataEFCore.Repositories;

public class UserRepository(JatodaContext context) : RepositoryBase<User>(context), IUserRepository
{
    private readonly JatodaContext _context = context;

    public void CreateUser(User user)
    {
        Create(user);
    }

    public void DeleteUser(User user)
    {
        Delete(user);
    }

    public void UpdateUser(User user)
    {
        Update(user);
    }

    public async Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges)
    {
        return await FindByCondition(x => ids.Contains(x.Id), trackChanges)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id, bool trackChanges)
    {
        return await FindByCondition(x => x.Id == id, trackChanges)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetByUsernameAsync(string username, bool trackChanges)
    {
        return await FindByCondition(x => x.Username == username, trackChanges)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email, bool trackChanges)
    {
        return await FindByCondition(x => x.Email == email, trackChanges)
            .FirstOrDefaultAsync();
    }
}