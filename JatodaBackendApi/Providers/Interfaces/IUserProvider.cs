namespace JatodaBackendApi.Providers.Interfaces;

public interface IUserProvider<T> where T : class
{
    Task<T?> GetByUsernameAsync(string username);
    Task<T> AddUserAsync(T entity);
    Task<T?> GetByEmailAsync(string email);
}