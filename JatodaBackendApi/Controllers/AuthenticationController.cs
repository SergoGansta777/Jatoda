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
        if (IsInvalidLoginRequest(model))
        {
            return HandleInvalidLoginRequest();
        }

        var user = await _userProvider.GetByUsernameAsync(model.Username);
        if (!IsValidUser(user, model.Password))
        {
            return HandleInvalidCredentials();
        }

        var token = _tokenService.GenerateToken(user.Id.ToString(), user.Username);
        SetAuthCookie(token);

        _logger.LogInformation("User {Username} logged in successfully.", user.Username);

        return Ok(new {message = "Login successful", username = user.Username, userid = user.Id});
    }

    [HttpPost("logout")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public IActionResult Logout()
    {
        if (Request.Cookies.TryGetValue("jwt", out var token))
        {
            RevokeUserToken(token);
            _logger.LogInformation($"User with token {token} logged out.");
            return Ok();
        }

        _logger.LogWarning("No jwt cookie found.");
        return BadRequest("No jwt cookie found.");
    }

    [HttpGet("user")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> UserByToken()
    {
        try
        {
            var jwt = Request.Cookies["jwt"];
            if (jwt == null)
            {
                return Unauthorized();
            }

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
        SetCorsHeaders();
        _logger.LogInformation("Responding to an OPTIONS request.");
        return NoContent();
    }

    private static bool IsInvalidLoginRequest(LoginRequestModelView? model)
    {
        return model == null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password);
    }

    private IActionResult HandleInvalidLoginRequest()
    {
        const string errorMessage = "Invalid payload. Username and Password should not be empty.";
        _logger.LogWarning(errorMessage);
        return BadRequest(errorMessage);
    }

    private static bool IsValidUser(User? user, string password)
    {
        return user != null && BCryptNet.Verify(password, user.Passwordhash);
    }

    private IActionResult HandleInvalidCredentials()
    {
        const string errorMessage = "Invalid credentials. Please check your username and password.";
        _logger.LogWarning(errorMessage);
        return BadRequest(errorMessage);
    }

    private void SetAuthCookie(string token)
    {
        var cookieOptions = new CookieOptions {HttpOnly = true};
        Response.Cookies.Append("jwt", token, cookieOptions);
    }


    private void RevokeUserToken(string token)
    {
        _tokenService.RevokeToken(token);
        Response.Cookies.Delete("jwt");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestModelView? model)
    {
        if (IsInvalidRegistrationRequest(model))
        {
            return HandleInvalidRegistrationPayload();
        }

        if (await IsUsernameTaken(model.Username))
        {
            return HandleUsernameAlreadyTaken();
        }

        if (await IsEmailTaken(model.Email))
        {
            return HandleEmailAlreadyInUse();
        }

        var createdUser = await CreateUser(model);

        _logger.LogInformation("User {Username} registered successfully.", createdUser.Username);

        return CreatedAtAction(nameof(Register), new {id = createdUser.Id}, createdUser);
    }

    private bool IsInvalidRegistrationRequest(RegisterRequestModelView? model)
    {
        return model == null
               || string.IsNullOrWhiteSpace(model.Username)
               || string.IsNullOrWhiteSpace(model.Email)
               || string.IsNullOrWhiteSpace(model.Password);
    }

    private IActionResult HandleInvalidRegistrationPayload()
    {
        const string errorMessage = "Invalid registration payload.";
        _logger.LogWarning(errorMessage);
        return BadRequest("Invalid payload.");
    }

    private async Task<bool> IsUsernameTaken(string username)
    {
        return await _userProvider.GetByUsernameAsync(username) != null;
    }

    private IActionResult HandleUsernameAlreadyTaken()
    {
        const string errorMessage = "Username is already taken";
        _logger.LogWarning(errorMessage);
        return BadRequest(errorMessage);
    }

    private async Task<bool> IsEmailTaken(string email)
    {
        return await _userProvider.GetByEmailAsync(email) != null;
    }

    private IActionResult HandleEmailAlreadyInUse()
    {
        const string errorMessage = "Email is already in use";
        _logger.LogWarning(errorMessage);
        return BadRequest(errorMessage);
    }

    private async Task<User> CreateUser(RegisterRequestModelView model)
    {
        var passwordHash = BCryptNet.HashPassword(model.Password);
        var user = new User
        {
            Username = model.Username,
            Passwordhash = passwordHash,
            Email = model.Email
        };
        return await _userProvider.AddUserAsync(user);
    }

    private void SetCorsHeaders()
    {
        Response.Headers.Add("Access-Control-Allow-Origin", "*");
        Response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
        Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
    }
}