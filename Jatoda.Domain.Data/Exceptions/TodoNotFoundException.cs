namespace Jatoda.Domain.Data.Exceptions;

public class TodoNotFoundException : NotFoundException
{
    public TodoNotFoundException(Guid id) : base($"Todo with id {id} not found in database")
    {
    }
}