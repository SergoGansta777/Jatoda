using JatodaBackendApi.Models.ModelViews;
using JatodaBackendApi.Services.AuthService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JatodaBackendApi.Controllers;

/// <summary>
///     Handles user authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    ///     Logs in a user with the specified credentials.
    /// </summary>
    /// <param name="model">The login request model containing username and password.</param>
    /// <returns>An action result indicating success or failure of the login attempt.</returns>
    /// <response code="200">Returns a success message along with user details.</response>
    /// <response code="400">Returns an error message if the login credentials are invalid.</response>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestModelView model)
    {
        return await _authService.Login(model);
    }

    /// <summary>
    ///     Logs out the currently logged-in user.
    /// </summary>
    /// <returns>An action result indicating the success or failure of the logout attempt.</returns>
    /// <response code="200">Returns a success message if logout is successful.</response>
    /// <response code="400">Returns an error message if no JWT cookie is found.</response>
    [HttpPost("logout")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public IActionResult Logout()
    {
        return _authService.Logout();
    }

    /// <summary>
    ///     Registers a new user with the specified details.
    /// </summary>
    /// <param name="model">The registration request model containing username, email, and password.</param>
    /// <returns>An action result indicating the success or failure of the registration attempt.</returns>
    /// <response code="201">Returns the created user details if registration is successful.</response>
    /// <response code="400">Returns an error message if the registration details are invalid.</response>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestModelView model)
    {
        return await _authService.Register(model);
    }

    /// <summary>
    ///     Gets the user details based on the JWT token.
    /// </summary>
    /// <returns>An action result containing the user details if the token is valid.</returns>
    /// <response code="200">Returns the user details if the token is valid.</response>
    /// <response code="401">Returns an error message if the token is invalid or expired.</response>
    [HttpGet("user")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> UserByToken()
    {
        return await _authService.GetUserByToken();
    }
}