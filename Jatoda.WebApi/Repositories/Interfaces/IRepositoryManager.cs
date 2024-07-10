namespace Jatoda.Repositories.Interfaces;

public interface IRepositoryManager
{
    IToDoRepository Todo { get; }
    ITagRepository Tag { get; }
    IUserRepository User { get; }
    void Save();
}