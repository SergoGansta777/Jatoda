using Jatoda.Domain.Data.DBModels;

namespace Jatoda.Providers.Interfaces;

public interface IUserProvider<T>
    where T : class
{
    Task<T?> GetByUsernameAsync(string? username);
    Task<T> CreateAsync(T entity);
    Task<T?> GetByEmailAsync(string? email);
    Task<User?> GetByIdAsync(Guid id);
}