namespace JatodaBackendApi.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(string userId, string username);
    bool ValidateToken(string token);

    void RevokeToken(string token);
    void ClearRevokedTokens();
}