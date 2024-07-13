using System.IdentityModel.Tokens.Jwt;

namespace Jatoda.Application.Interfaces;

public interface ITokenService
{
    string? GenerateToken(string? userId, string? username);
    JwtSecurityToken ValidateToken(string? token);
    void RevokeToken(string? token);
    string GetUserIdFromToken(string? token);
    void ClearRevokedTokens();
}