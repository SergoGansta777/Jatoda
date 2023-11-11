namespace JatodaBackendApi.Providers.Interfaces;

public interface IUserProvider<T> where T : class
{
    Task<T?> GetByUsernameAsync(string username);
}