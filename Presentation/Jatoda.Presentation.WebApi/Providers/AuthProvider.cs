using System.Security.Claims;
using Jatoda.Application.Core.Models.ModelViews;
using Jatoda.Application.Core.Models.ResponseModels;
using Jatoda.Application.Interfaces;
using Jatoda.Domain.Core.DBModels;
using Jatoda.Providers.Interfaces;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Jatoda.Providers;

public class AuthProvider : IAuthProvider
{
    private readonly IEmailConfirmationService _emailConfirmationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthProvider> _logger;
    private readonly ITokenService _tokenService;
    private readonly IUserProvider<User> _userProvider;

    public AuthProvider(
        IUserProvider<User> userProvider,
        ITokenService tokenService,
        ILogger<AuthProvider> logger,
        IHttpContextAccessor httpContextAccessor,
        IEmailConfirmationService emailConfirmationService)
    {
        _userProvider = userProvider;
        _tokenService = tokenService;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _emailConfirmationService = emailConfirmationService;
    }

    public async Task<AuthResponseModel> Login(LoginRequestModel? model)
    {
        if (IsInvalidLoginRequest(model))
        {
            return new AuthResponseModel
                {Success = false, Message = "Invalid payload. Username and Password should not be empty."};
        }

        var user = await _userProvider.GetByUsernameAsync(model.Username);
        if (!IsValidUser(user, model.Password))
        {
            return new AuthResponseModel
                {Success = false, Message = "Invalid credentials. Please check your username and password."};
        }

        var token = _tokenService.GenerateToken(user?.Id.ToString(), user?.Username);
        SetAuthCookie(token);

        _logger.LogInformation("User {Username} logged in successfully.", user.Username);

        return new AuthResponseModel {Success = true, Message = "Login successful", User = user, Token = token};
    }

    public LogoutResponseModel Logout()
    {
        if (_httpContextAccessor.HttpContext != null &&
            _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("jwt", out var token))
        {
            RevokeUserToken(token);
            _logger.LogInformation("User with token {token} logged out.", token);
            return new LogoutResponseModel {Success = true, Message = "Logout successful."};
        }

        _logger.LogWarning("No jwt cookie found.");
        return new LogoutResponseModel {Success = false, Message = "No jwt cookie found."};
    }

    public async Task<AuthResponseModel> Register(RegisterRequestModel? model)
    {
        if (IsInvalidRegistrationRequest(model))
        {
            return new AuthResponseModel {Success = false, Message = "Invalid payload."};
        }

        if (await IsUsernameTaken(model!.Username))
        {
            return new AuthResponseModel {Success = false, Message = "Username is already taken"};
        }

        if (await IsEmailTaken(model.Email))
        {
            return new AuthResponseModel {Success = false, Message = "Email is already in use"};
        }

        var createdUser = await CreateUser(model);
        _logger.LogInformation("User {Username} registered successfully.", createdUser.Username);

        await _emailConfirmationService.SendVerificationEmail(createdUser);

        return new AuthResponseModel {Success = true, Message = "User registered successfully.", User = createdUser};
    }

    public async Task<AuthResponseModel> GetUserByToken()
    {
        try
        {
            var jwt = _httpContextAccessor.HttpContext?.Request.Cookies["jwt"];
            if (jwt is null)
            {
                return new AuthResponseModel {Success = false, Message = "Unauthorized"};
            }

            var token = _tokenService.ValidateToken(jwt);
            var userId = Guid.Parse(token.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            var user = await _userProvider.GetByIdAsync(userId);
            return new AuthResponseModel {Success = true, Message = "User retrieved successfully.", User = user};
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching user.");
            return new AuthResponseModel {Success = false, Message = "Unauthorized"};
        }
    }

    public async Task<AuthResponseModel> ConfirmEmail(string? token)
    {
        var result = new AuthResponseModel();
        var isValid = await _emailConfirmationService.ConfirmEmail(token);
        if (!isValid)
        {
            result.Success = false;
            result.Message = "Invalid or expired token.";
            return result;
        }

        result.Success = true;
        result.Message = "Email confirmed successfully.";
        return result;
    }

    private static bool IsInvalidLoginRequest(LoginRequestModel? model)
    {
        return model is null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password);
    }

    private static bool IsValidUser(User? user, string? password)
    {
        return user is not null && BCryptNet.Verify(password, user.PasswordHash);
    }

    private void SetAuthCookie(string? token)
    {
        var cookieOptions = new CookieOptions {HttpOnly = true};
        _httpContextAccessor.HttpContext?.Response.Cookies.Append("jwt", token, cookieOptions);
    }

    private void RevokeUserToken(string token)
    {
        _tokenService.RevokeToken(token);
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete("jwt");
    }

    private static bool IsInvalidRegistrationRequest(RegisterRequestModel? model)
    {
        return model is null || string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Email) ||
               string.IsNullOrWhiteSpace(model.Password);
    }

    private async Task<bool> IsUsernameTaken(string? username)
    {
        return await _userProvider.GetByUsernameAsync(username) is not null;
    }

    private async Task<bool> IsEmailTaken(string? email)
    {
        return await _userProvider.GetByEmailAsync(email) is not null;
    }

    private async Task<User> CreateUser(RegisterRequestModel model)
    {
        var passwordHash = BCryptNet.HashPassword(model.Password);
        var user = new User
        {
            Username = model.Username,
            PasswordHash = passwordHash,
            Email = model.Email
        };

        await _userProvider.CreateAsync(user);
        return user;
    }
}