using Jatoda.Application.Core.Models.ModelViews;
using Jatoda.Application.Interfaces;
using Jatoda.Domain.Data.DBModels;
using Jatoda.Providers.Interfaces;
using Microsoft.AspNetCore.Mvc;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Jatoda.Providers;

public class AuthProvider(
    IUserProvider<User> userProvider,
    ITokenService tokenService,
    ILogger<AuthProvider> logger,
    IHttpContextAccessor httpContextAccessor)
    : IAuthProvider
{
    public async Task<IActionResult> Login(LoginRequestModelView? model)
    {
        if (IsInvalidLoginRequest(model))
        {
            return HandleInvalidLoginRequest();
        }

        var user = await userProvider.GetByUsernameAsync(model.Username);
        if (!IsValidUser(user, model.Password))
        {
            return HandleInvalidCredentials();
        }

        var token = tokenService.GenerateToken(user?.Id.ToString(), user?.Username);
        SetAuthCookie(token);

        logger.LogInformation("User {Username} logged in successfully.", user.Username);

        return new OkObjectResult(new {message = "Login successful", username = user.Username, token});
    }

    public IActionResult Logout()
    {
        if (httpContextAccessor.HttpContext != null &&
            httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("jwt", out var token))
        {
            RevokeUserToken(token);
            logger.LogInformation("User with token {token} logged out.", token);
            return new OkResult();
        }

        logger.LogWarning("No jwt cookie found.");
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
        logger.LogInformation("User {Username} registered successfully.", createdUser.Username);

        return new CreatedAtActionResult(nameof(Register), "Auth", new {id = createdUser.Id}, createdUser);
    }

    public async Task<IActionResult> GetUserByToken()
    {
        try
        {
            var jwt = httpContextAccessor.HttpContext?.Request.Cookies["jwt"];
            if (jwt is null)
            {
                return new UnauthorizedResult();
            }

            var token = tokenService.ValidateToken(jwt);
            var userId = Guid.Parse(token.Payload.First(c => c.Key == "nameid").Value.ToString()!);
            var user = await userProvider.GetByIdAsync(userId);
            return new OkObjectResult(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching user.");
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
        logger.LogWarning(errorMessage);
        return new BadRequestObjectResult(errorMessage);
    }

    private static bool IsValidUser(User? user, string? password)
    {
        return user is not null && BCryptNet.Verify(password, user.PasswordHash);
    }

    private BadRequestObjectResult HandleInvalidCredentials()
    {
        const string errorMessage = "Invalid credentials. Please check your username and password.";
        logger.LogWarning(errorMessage);
        return new BadRequestObjectResult(errorMessage);
    }

    private void SetAuthCookie(string token)
    {
        var cookieOptions = new CookieOptions {HttpOnly = true};
        httpContextAccessor.HttpContext?.Response.Cookies.Append("jwt", token, cookieOptions);
    }

    private void RevokeUserToken(string token)
    {
        tokenService.RevokeToken(token);
        httpContextAccessor.HttpContext?.Response.Cookies.Delete("jwt");
    }

    private static bool IsInvalidRegistrationRequest(RegisterRequestModelView? model)
    {
        return model is null || string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Email) ||
               string.IsNullOrWhiteSpace(model.Password);
    }

    private IActionResult HandleInvalidRegistrationPayload()
    {
        const string errorMessage = "Invalid registration payload.";
        logger.LogWarning(errorMessage);
        return new BadRequestObjectResult("Invalid payload.");
    }

    private async Task<bool> IsUsernameTaken(string? username)
    {
        return await userProvider.GetByUsernameAsync(username) is not null;
    }

    private BadRequestObjectResult HandleUsernameAlreadyTaken()
    {
        const string errorMessage = "Username is already taken";
        logger.LogWarning(errorMessage);

        return new BadRequestObjectResult(errorMessage);
    }

    private async Task<bool> IsEmailTaken(string? email)
    {
        return await userProvider.GetByEmailAsync(email) is not null;
    }

    private BadRequestObjectResult HandleEmailAlreadyInUse()
    {
        const string errorMessage = "Email is already in use";
        logger.LogWarning(errorMessage);

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

        return await userProvider.AddUserAsync(user);
    }
}