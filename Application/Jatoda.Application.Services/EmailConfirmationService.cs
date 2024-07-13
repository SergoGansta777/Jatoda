using System.Web;
using Jatoda.Application.Interfaces;
using Jatoda.Domain.Data.DBModels;
using Jatoda.Domain.Data.Options;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Jatoda.Application.Service;

public class EmailConfirmationService : IEmailConfirmationService
{
    private readonly EmailConfirmationOptions _emailOptions;
    private readonly ILogger<EmailConfirmationService> _logger;
    private readonly IRepositoryManager _repositoryManager;
    private readonly ITokenService _tokenService;

    public EmailConfirmationService(
        IOptions<EmailConfirmationOptions> emailOptions,
        ITokenService tokenService,
        IRepositoryManager repositoryManager,
        ILogger<EmailConfirmationService> logger)
    {
        _emailOptions = emailOptions.Value;
        _tokenService = tokenService;
        _repositoryManager = repositoryManager;
        _logger = logger;
    }

    public async Task SendVerificationEmail(User user)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("JatodaNoReply", _emailOptions.FromEmail));
        emailMessage.To.Add(new MailboxAddress(user.Username, user.Email));
        emailMessage.Subject = "Email Verification";

        var verificationLink =
            $"{_emailOptions.FrontendUrl}/verify-email?token={HttpUtility.UrlEncode(_tokenService.GenerateToken(user.Id.ToString(), user.Email))}";

        emailMessage.Body = new TextPart("plain")
        {
            Text = $"Please verify your email by clicking on the following link: {verificationLink}"
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(_emailOptions.SmtpServer, _emailOptions.SmtpPort, true);
        await client.AuthenticateAsync(_emailOptions.SmtpUsername, _emailOptions.SmtpPassword);
        await client.SendAsync(emailMessage);
        await client.DisconnectAsync(true);
    }

    public async Task<bool> ConfirmEmail(string token)
    {
        _tokenService.ValidateToken(token);

        var userId = _tokenService.GetUserIdFromToken(token);
        var user = await _repositoryManager.User.GetByIdAsync(Guid.Parse(userId), false);

        if (user is null || user.IsEmailConfirmed)
        {
            _logger.LogWarning("Email confirmation failed: user not found or already verified.");
            return false;
        }

        user.IsEmailConfirmed = true;
        _repositoryManager.User.UpdateUser(user);
        _repositoryManager.Save();

        _logger.LogInformation("User {Username} email verified successfully.", user.Username);
        return true;
    }
}