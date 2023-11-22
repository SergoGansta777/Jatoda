using JatodaBackendApi.Models.DBModels;
using JatodaBackendApi.Models.ModelViews;
using JatodaBackendApi.Providers.Interfaces;
using JatodaBackendApi.Services.JwtTokenService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BCryptNet = BCrypt.Net.BCrypt;

namespace JatodaBackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly ILogger<AuthenticationController> _logger;
    private readonly ITokenService _tokenService;
    private readonly IUserProvider<User> _userProvider;

    public AuthenticationController(
        IUserProvider<User> userProvider,
        ITokenService tokenService,
        ILogger<AuthenticationController> logger
    )
    {
        _userProvider = userProvider;
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestModelView? model)
    {
        if (
            model == null
            || string.IsNullOrEmpty(model.Username)
            || string.IsNullOrEmpty(model.Password)
        )
        {
            _logger.LogWarning("Invalid payload. Username and Password should not be empty.");
            return BadRequest("Invalid payload. Username and Password should not be empty.");
        }

        var user = await _userProvider.GetByUsernameAsync(model.Username);
        if (user == null || !BCryptNet.Verify(model.Password, user.Passwordhash))
        {
            _logger.LogWarning("Invalid credentials. Please check your username and password.");
            return BadRequest("Invalid credentials. Please check your username and password.");
        }

        var token = _tokenService.GenerateToken(user.Id.ToString(), user.Username);
        Response.Cookies.Append("jwt", token, new CookieOptions {HttpOnly = true});

        _logger.LogInformation("User {Username} logged in successfully.", user.Username);

        return Ok(
            new
            {
                message = "Login successful",
                username = user.Username,
                userid = user.Id
            }
        );
    }

    [HttpPost("logout")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public IActionResult Logout()
    {
        if (Request.Cookies.TryGetValue("jwt", out var token))
        {
            _tokenService.RevokeToken(token);
            Response.Cookies.Delete("jwt");

            _logger.LogInformation($"User with token {token} logged out.");

            return Ok();
        }

        _logger.LogWarning("No jwt cookie found.");
        return BadRequest("No jwt cookie found.");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestModelView? model)
    {
        if (
            model == null
            || string.IsNullOrWhiteSpace(model.Username)
            || string.IsNullOrWhiteSpace(model.Email)
            || string.IsNullOrWhiteSpace(model.Password)
        )
        {
            _logger.LogWarning("Invalid registration payload.");
            return BadRequest("Invalid payload.");
        }

        if (await _userProvider.GetByUsernameAsync(model.Username) != null)
        {
            _logger.LogWarning("Attempt to register with an existing username.");
            return BadRequest("Username is already taken");
        }

        if (await _userProvider.GetByEmailAsync(model.Email) != null)
        {
            _logger.LogWarning("Attempt to register with an existing email.");
            return BadRequest("Email is already in use");
        }

        var passwordHash = BCryptNet.HashPassword(model.Password);
        var user = new User
        {
            Username = model.Username,
            Passwordhash = passwordHash,
            Email = model.Email
        };

        var createdUser = await _userProvider.AddUserAsync(user);

        _logger.LogInformation("User {Username} registered successfully.", user.Username);

        return CreatedAtAction(
            nameof(Register),
            new {id = user.Id},
            createdUser
        );
    }

    [HttpGet("user")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> UserByToken()
    {
        try
        {
            var jwt = Request.Cookies["jwt"];
            if (jwt == null) return Unauthorized();
            var token = _tokenService.ValidateToken(jwt);
            var userId = int.Parse(token.Payload.First(c => c.Key == "nameid").Value.ToString()!);
            var user = await _userProvider.GetByIdAsync(userId);
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching user.");
            return Unauthorized();
        }
    }

    [HttpOptions]
    public IActionResult Options()
    {
        // Set the necessary CORS headers
        Response.Headers.Add("Access-Control-Allow-Origin", "*");
        Response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
        Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

        _logger.LogInformation("Responding to an OPTIONS request.");

        // Return a 204 No Content response
        return NoContent();
    }
}