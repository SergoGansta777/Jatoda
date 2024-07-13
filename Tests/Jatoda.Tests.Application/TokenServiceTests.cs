using System.Security.Claims;
using Jatoda.Application.Service;
using Jatoda.Domain.Core.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;
    private readonly Mock<IOptions<TokenOptions>> _mockOptions;

    public TokenServiceTests()
    {
        var tokenOptions = new TokenOptions
        {
            SecretKey = "your-256-bit-secret-needed-for-creation-token",
            TokenExpiryInDays = 1 
        };

        _mockOptions = new Mock<IOptions<TokenOptions>>();
        _mockOptions.Setup(o => o.Value).Returns(tokenOptions);

        _tokenService = new TokenService(_mockOptions.Object);
    }

    [Fact]
    public void GenerateToken_ValidInputs_ReturnsToken()
    {
        // Arrange
        const string userId = "123";
        const string username = "testuser";

        // Act
        var token = _tokenService.GenerateToken(userId, username);

        // Assert
        Assert.False(string.IsNullOrEmpty(token));
    }

    [Fact]
    public void GenerateToken_EmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        const string userId = "";
        const string username = "testuser";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _tokenService.GenerateToken(userId, username));
    }

    [Fact]
    public void ValidateToken_RevokedToken_ThrowsSecurityTokenException()
    {
        // Arrange
        var userId = "123";
        var username = "testuser";
        var token = _tokenService.GenerateToken(userId, username);
        _tokenService.RevokeToken(token);

        // Act & Assert
        Assert.Throws<SecurityTokenException>(() => _tokenService.ValidateToken(token));
    }

    [Fact]
    public void GetUserIdFromToken_ValidToken_ReturnsUserId()
    {
        // Arrange
        var userId = "123";
        var username = "testuser";
        var token = _tokenService.GenerateToken(userId, username);

        // Act
        var resultUserId = _tokenService.GetUserIdFromToken(token);

        // Assert
        Assert.Equal(userId, resultUserId);
    }

    [Fact]
    public void RevokeToken_ValidToken_AddsToRevokedList()
    {
        // Arrange
        var userId = "123";
        var username = "testuser";
        var token = _tokenService.GenerateToken(userId, username);

        // Act
        _tokenService.RevokeToken(token);

        // Assert
        Assert.Throws<SecurityTokenException>(() => _tokenService.ValidateToken(token));
    }

    [Fact]
    public void ClearRevokedTokens_ClearsRevokedList()
    {
        // Arrange
        var userId = "123";
        var username = "testuser";
        var token = _tokenService.GenerateToken(userId, username);
        _tokenService.RevokeToken(token);

        // Act
        _tokenService.ClearRevokedTokens();
        
        // Assert
        var jwtToken = _tokenService.ValidateToken(token);
        Assert.NotNull(jwtToken);
    }
}