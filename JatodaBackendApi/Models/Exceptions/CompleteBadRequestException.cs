namespace JatodaBackendApi.Models.Exceptions;

public class CompleteBadRequestException : BadRequestException
{
    public CompleteBadRequestException() : base("Error in request body for complete todo")
    {
    }
}