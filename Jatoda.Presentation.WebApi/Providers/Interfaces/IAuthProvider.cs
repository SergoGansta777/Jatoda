using Jatoda.Application.Core.Models.ModelViews;
using Microsoft.AspNetCore.Mvc;

namespace Jatoda.Providers.Interfaces;

public interface IAuthProvider
{
    Task<IActionResult> Login(LoginRequestModelView? model);
    IActionResult Logout();
    Task<IActionResult> Register(RegisterRequestModelView? model);
    Task<IActionResult> GetUserByToken();
}