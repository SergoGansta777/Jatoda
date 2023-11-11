using System.Security.Cryptography;
using System.Text;

using JatodaBackendApi.Model;
using Microsoft.AspNetCore.Mvc;
using JatodaBackendApi.Providers.Interfaces;
using JatodaBackendApi.Services.Interfaces;

namespace JatodaBackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IUserProvider<User> _userProvider;
    private readonly ITokenService _tokenService;
    
    public AuthenticationController(IUserProvider<User> userProvider, ITokenService tokenService)
    {
        _userProvider = userProvider;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest model)
    {
        var user = await _userProvider.GetByUsernameAsync(model.Username);
        if (user == null)
        {
            return Unauthorized();
        }

        return Unauthorized();
    }
    
    private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    {
        using var hmac = new HMACSHA512(storedSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != storedHash[i])
            {
                return false;
            }
        }

        return true;
    }
}