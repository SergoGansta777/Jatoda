namespace Jatoda.Domain.Core.Options;

public class EmailConfirmationOptions
{
    public string FromEmail { get; set; }
    public string SmtpServer { get; set; }
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; }
    public string SmtpPassword { get; set; }
    public string FrontendUrl { get; set; }
}