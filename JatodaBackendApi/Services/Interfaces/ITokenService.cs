namespace JatodaBackendApi.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(string userId, string username);
}