using System.IdentityModel.Tokens.Jwt;

namespace Jatoda.Services.JwtTokenService.Interfaces;

public interface ITokenService
{
    string GenerateToken(string? userId, string? username);
    JwtSecurityToken ValidateToken(string? token);

    void RevokeToken(string? token);
    void ClearRevokedTokens();
}