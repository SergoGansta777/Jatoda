using Jatoda.Domain.Data.DBModels;

namespace Jatoda.Application.Interfaces;

public interface IEmailConfirmationService
{
    Task SendVerificationEmail(User user);
    Task<bool> ConfirmEmail(string token);
}