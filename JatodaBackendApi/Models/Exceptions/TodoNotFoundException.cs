namespace JatodaBackendApi.Models.Exceptions;

public class TodoNotFoundException : NotFoundException
{
    public TodoNotFoundException(int id) : base($"Todo with id {id} not found in database")
    {
    }
}