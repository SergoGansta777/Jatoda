using Jatoda.Application.Interfaces;

namespace Jatoda.Infrastructure.DataEFCore.Repositories;

public sealed class RepositoryManager(JatodaContext context) : IRepositoryManager
{
    private readonly Lazy<ITagRepository> _tagRepository = new(() => new TagRepository(context));
    private readonly Lazy<IToDoRepository> _todoRepository = new(() => new ToDoRepository(context));
    private readonly Lazy<IUserRepository> _userRepository = new(() => new UserRepository(context));

    public ITagRepository Tag => _tagRepository.Value;
    public IUserRepository User => _userRepository.Value;
    public IToDoRepository Todo => _todoRepository.Value;

    public void Save()
    {
        context.SaveChanges();
    }
}