using JatodaBackendApi.Models.DBModels;
using JatodaBackendApi.Providers.Interfaces;
using JatodaBackendApi.Repositories.Interfaces;
using static System.DateTime;

namespace JatodaBackendApi.Providers;

public class UserProvider(IRepositoryManager repositoryManager)
    : IUserProvider<User>
{
    public Task<User> AddUserAsync(User user)
    {
        user.CreateDate = Now.ToUniversalTime();
        user.UpdateDate = Now.ToUniversalTime();

        repositoryManager.User.CreateUser(user);
        repositoryManager.Save();

        return Task.FromResult(user);
    }

    public async Task<User?> GetByUsernameAsync(string? username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return null;
        }

        var user = await repositoryManager.User.GetByUsernameAsync(username, false);

        return user;
    }

    public async Task<User?> GetByEmailAsync(string? email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return null;
        }

        var user = await repositoryManager.User.GetByEmailAsync(email, false);
        return user;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        var user = await repositoryManager.User.GetByIdAsync(id, false);
        return user;
    }
}