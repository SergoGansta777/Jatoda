using Jatoda.Application.Interfaces;
using Jatoda.Domain.Core.DBModels;
using Jatoda.Providers.Interfaces;
using static System.DateTime;

namespace Jatoda.Providers;

public class UserProvider(IRepositoryManager repositoryManager)
    : IUserProvider<User>
{
    public Task<User> CreateAsync(User user)
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