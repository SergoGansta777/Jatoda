using JatodaBackendApi.Models.ModelViews;
using Microsoft.AspNetCore.Mvc;

namespace JatodaBackendApi.Services.AuthService.Interfaces;

public interface IAuthService
{
    Task<IActionResult> Login(LoginRequestModelView? model);
    IActionResult Logout();
    Task<IActionResult> Register(RegisterRequestModelView? model);
    Task<IActionResult> GetUserByToken();
}