using JatodaBackendApi.Models.DBModels;
using JatodaBackendApi.Models.ModelViews;
using JatodaBackendApi.Providers.Interfaces;
using JatodaBackendApi.Services.AuthService.Interfaces;
using JatodaBackendApi.Services.JwtTokenService.Interfaces;
using Microsoft.AspNetCore.Mvc;
using BCryptNet = BCrypt.Net.BCrypt;

namespace JatodaBackendApi.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthService> _logger;
    private readonly ITokenService _tokenService;
    private readonly IUserProvider<User> _userProvider;

    public AuthService(IUserProvider<User> userProvider, ITokenService tokenService, ILogger<AuthService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _userProvider = userProvider;
        _tokenService = tokenService;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

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

        var token = _tokenService.GenerateToken(user?.Id.ToString(), user?.Username);
        SetAuthCookie(token);

        _logger.LogInformation("User {Username} logged in successfully.", user.Username);

        return new OkObjectResult(new {message = "Login successful", username = user.Username, userid = user.Id});
    }

    public IActionResult Logout()
    {
        if (_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("jwt", out var token))
        {
            RevokeUserToken(token);
            _logger.LogInformation("User with token {token} logged out.", token);
            return new OkResult();
        }

        _logger.LogWarning("No jwt cookie found.");
        return new BadRequestObjectResult("No jwt cookie found.");
    }

    public async Task<IActionResult> Register(RegisterRequestModelView? model)
    {
        if (IsInvalidRegistrationRequest(model))
        {
            return HandleInvalidRegistrationPayload();
        }

        if (await IsUsernameTaken(model!.Username))
        {
            return HandleUsernameAlreadyTaken();
        }

        if (await IsEmailTaken(model.Email))
        {
            return HandleEmailAlreadyInUse();
        }

        var createdUser = await CreateUser(model);
        _logger.LogInformation("User {Username} registered successfully.", createdUser.Username);

        return new CreatedAtActionResult(nameof(Register), "Auth", new {id = createdUser.Id}, createdUser);
    }

    public async Task<IActionResult> GetUserByToken()
    {
        try
        {
            var jwt = _httpContextAccessor.HttpContext?.Request.Cookies["jwt"];
            if (jwt is null)
            {
                return new UnauthorizedResult();
            }

            var token = _tokenService.ValidateToken(jwt);
            var userId = int.Parse(token.Payload.First(c => c.Key == "nameid").Value.ToString()!);
            var user = await _userProvider.GetByIdAsync(userId);
            return new OkObjectResult(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching user.");
            return new UnauthorizedResult();
        }
    }

    private static bool IsInvalidLoginRequest(LoginRequestModelView? model)
    {
        return model is null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password);
    }

    private IActionResult HandleInvalidLoginRequest()
    {
        const string errorMessage = "Invalid payload. Username and Password should not be empty.";
        _logger.LogWarning(errorMessage);
        return new BadRequestObjectResult(errorMessage);
    }

    private static bool IsValidUser(User? user, string password)
    {
        return user is not null && BCryptNet.Verify(password, user.PasswordHash);
    }

    private BadRequestObjectResult HandleInvalidCredentials()
    {
        const string errorMessage = "Invalid credentials. Please check your username and password.";
        _logger.LogWarning(errorMessage);
        return new BadRequestObjectResult(errorMessage);
    }

    private void SetAuthCookie(string token)
    {
        var cookieOptions = new CookieOptions {HttpOnly = true};
        _httpContextAccessor.HttpContext?.Response.Cookies.Append("jwt", token, cookieOptions);
    }

    private void RevokeUserToken(string token)
    {
        _tokenService.RevokeToken(token);
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete("jwt");
    }

    private static bool IsInvalidRegistrationRequest(RegisterRequestModelView? model)
    {
        return model is null || string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Email) ||
               string.IsNullOrWhiteSpace(model.Password);
    }

    private IActionResult HandleInvalidRegistrationPayload()
    {
        const string errorMessage = "Invalid registration payload.";
        _logger.LogWarning(errorMessage);
        return new BadRequestObjectResult("Invalid payload.");
    }

    private async Task<bool> IsUsernameTaken(string? username)
    {
        return await _userProvider.GetByUsernameAsync(username) is not null;
    }

    private BadRequestObjectResult HandleUsernameAlreadyTaken()
    {
        const string errorMessage = "Username is already taken";
        _logger.LogWarning(errorMessage);

        return new BadRequestObjectResult(errorMessage);
    }

    private async Task<bool> IsEmailTaken(string? email)
    {
        return await _userProvider.GetByEmailAsync(email) is not null;
    }

    private BadRequestObjectResult HandleEmailAlreadyInUse()
    {
        const string errorMessage = "Email is already in use";
        _logger.LogWarning(errorMessage);

        return new BadRequestObjectResult(errorMessage);
    }

    private async Task<User> CreateUser(RegisterRequestModelView model)
    {
        var passwordHash = BCryptNet.HashPassword(model.Password);
        var user = new User
        {
            Username = model.Username,
            PasswordHash = passwordHash,
            Email = model.Email
        };

        return await _userProvider.AddUserAsync(user);
    }
}