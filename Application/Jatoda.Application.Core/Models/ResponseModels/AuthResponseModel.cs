using Jatoda.Domain.Data.DBModels;

namespace Jatoda.Application.Core.Models.ResponseModels;

public class AuthResponseModel
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public User? User { get; set; }
    public string? Token { get; set; }
}