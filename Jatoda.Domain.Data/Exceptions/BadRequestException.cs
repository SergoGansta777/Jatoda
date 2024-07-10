namespace Jatoda.Domain.Data.Exceptions;

public abstract class BadRequestException(string message) : Exception(message);