using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;
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

        if (!VerifyPasswordHash(model.Password, user.Passwordhash, user.Passwordsalt))
        {
            return Unauthorized();
        }

        var token = _tokenService.GenerateToken(user.Id.ToString(), user.Username);
        return Ok(new
        {
            Token = token
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest model)
    {
        if (await _userProvider.GetByUsernameAsync(model.Username) != null)
        {
            return BadRequest("Username is already taken");
        }

        var (passwordHash, passwordSalt) = CreatePasswordHash(model.Password);
        var user = new User
        {
            Username = model.Username,
            Passwordhash = passwordHash!,
            Passwordsalt = passwordSalt!
        };

        await _userProvider.AddUserAsync(user);
        return Ok();
    }
    
    private bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(storedSalt));
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
    
    private (string?, string?) CreatePasswordHash(string password)
    {
        using var hmac = new HMACSHA512();
        var passwordSalt = hmac.Key;
        var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        return (passwordHash.ToString(), passwordSalt.ToString());
    }
}