using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Jatoda.Application.Interfaces;
using Jatoda.Domain.Data.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Jatoda.Application.Service;

public class TokenService(IOptions<TokenOptions> options) : ITokenService
{
    private readonly TokenOptions _options = options.Value;
    private readonly List<string?> _revokedToken = [];

    public string GenerateToken(string? userId, string? username)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenExpiryTime = _options.TokenExpiry;
            var secretKey = _options.SecretKey;
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("Secret key must be provided.");
            }

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Empty user id or username");
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId),
                        new Claim(ClaimTypes.Name, username)
                    }
                ),
                Expires = DateTime.UtcNow.AddDays(tokenExpiryTime),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return string.Empty;
        }
    }

    public JwtSecurityToken ValidateToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new ArgumentNullException(nameof(token));
        }

        if (_revokedToken.Contains(token))
        {
            throw new SecurityTokenException("Token revoked.");
        }

        var tokenHandler = new JwtSecurityTokenHandler();

        if (!tokenHandler.CanReadToken(token))
        {
            throw new ArgumentException("Invalid JWT token format.");
        }

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_options.SecretKey)
            )
        };

        try
        {
            tokenHandler.ValidateToken(
                token,
                validationParameters,
                out var validatedToken
            );

            if (
                validatedToken is not JwtSecurityToken jwtToken
                || !jwtToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase
                )
            )
            {
                throw new ArgumentException("Invalid JWT token encryption.");
            }

            return jwtToken;
        }
        catch (Exception ex)
        {
            throw new SecurityTokenException("Token validation failed.", ex);
        }
    }

    public string GetUserIdFromToken(string token)
    {
        var jwtToken = ValidateToken(token);
        return jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
    }

    public void RevokeToken(string? token)
    {
        _revokedToken.Add(token);
    }

    public void ClearRevokedTokens()
    {
        _revokedToken.Clear();
    }
}