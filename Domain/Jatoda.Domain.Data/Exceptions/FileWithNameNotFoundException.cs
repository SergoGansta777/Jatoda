namespace Jatoda.Domain.Data.Exceptions;

public class FileWithNameNotFoundException : NotFoundException
{
    public FileWithNameNotFoundException(string fileName) : base($"File with {fileName} name not found in datebase")
    {
    }
}