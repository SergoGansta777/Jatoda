namespace JatodaBackendApi.Models.Exceptions;

public abstract class BadRequestException(string message) : Exception(message);