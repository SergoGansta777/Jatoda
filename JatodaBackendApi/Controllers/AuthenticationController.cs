using JatodaBackendApi.Models.ModelViews;
using JatodaBackendApi.Services.AuthService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JatodaBackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestModelView? model)
    {
        return await _authService.Login(model);
    }

    [HttpPost("logout")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public IActionResult Logout()
    {
        return _authService.Logout();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestModelView? model)
    {
        return await _authService.Register(model);
    }

    [HttpGet("user")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> UserByToken()
    {
        return await _authService.GetUserByToken();
    }

    [HttpOptions]
    public IActionResult Options()
    {
        SetCorsHeaders();
        return NoContent();
    }

    private void SetCorsHeaders()
    {
        Response.Headers.Add("Access-Control-Allow-Origin", "*");
        Response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
        Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
    }
}