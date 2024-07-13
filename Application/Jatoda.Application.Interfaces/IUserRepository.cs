using Jatoda.Domain.Core.DBModels;

namespace Jatoda.Application.Interfaces;

public interface IUserRepository
{
    void CreateUser(User user);
    void DeleteUser(User user);
    void UpdateUser(User user);

    Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges);
    Task<User?> GetByIdAsync(Guid id, bool trackChanges);
    Task<User?> GetByUsernameAsync(string username, bool trackChanges);
    Task<User?> GetByEmailAsync(string email, bool trackChanges);
}