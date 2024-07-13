using Jatoda.Application.Core.Models.ModelViews;
using Jatoda.Application.Core.Models.ResponseModels;

namespace Jatoda.Providers.Interfaces;

public interface IAuthProvider
{
    Task<AuthResponseModel> Login(LoginRequestModel? model);
    LogoutResponseModel Logout();
    Task<AuthResponseModel> Register(RegisterRequestModel? model);
    Task<AuthResponseModel> GetUserByToken();
    Task<AuthResponseModel> ConfirmEmail(string? token);
}