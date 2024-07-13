using Jatoda.Application.Core.Models.ModelViews;
using Jatoda.Providers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jatoda.Controllers;

/// <summary>
///     Handles user authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthProvider authProvider) : ControllerBase
{
    /// <summary>
    ///     Logs in a user with the specified credentials.
    /// </summary>
    /// <param name="model">The login request model containing username and password.</param>
    /// <returns>An action result indicating success or failure of the login attempt.</returns>
    /// <response code="200">Returns a success message along with user details.</response>
    /// <response code="400">Returns an error message if the login credentials are invalid.</response>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestModel? model)
    {
        var result = await authProvider.Login(model);
        if (!result.Success)
        {
            return BadRequest(new {message = result.Message});
        }

        return Ok(new {message = result.Message, username = result.User?.Username, token = result.Token});
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
        var result = authProvider.Logout();
        if (!result.Success)
        {
            return BadRequest(new {message = result.Message});
        }

        return Ok(new {message = result.Message});
    }

    /// <summary>
    ///     Registers a new user with the specified details.
    /// </summary>
    /// <param name="model">The registration request model containing username, email, and password.</param>
    /// <returns>An action result indicating the success or failure of the registration attempt.</returns>
    /// <response code="201">Returns the created user details if registration is successful.</response>
    /// <response code="400">Returns an error message if the registration details are invalid.</response>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestModel model)
    {
        var result = await authProvider.Register(model);
        if (!result.Success)
        {
            return BadRequest(new {message = result.Message});
        }

        return CreatedAtAction(nameof(Register), new {id = result.User?.Id}, result.User);
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
        var result = await authProvider.GetUserByToken();
        if (!result.Success)
        {
            return Unauthorized(new {message = result.Message});
        }

        return Ok(result.User);
    }

    /// <summary>
    ///     Confirms a user's email using the provided token.
    /// </summary>
    /// <param name="token">The email confirmation token.</param>
    /// <returns>An action result indicating the success or failure of the email confirmation.</returns>
    /// <response code="200">Returns a success message if email confirmation is successful.</response>
    /// <response code="400">Returns an error message if the token is invalid or expired.</response>
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
    {
        var result = await authProvider.ConfirmEmail(token);
        if (!result.Success)
        {
            return BadRequest(new {message = result.Message});
        }

        return Ok(new {message = result.Message});
    }
}