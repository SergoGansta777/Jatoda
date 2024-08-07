namespace Jatoda.Domain.Core.Options;

public class TokenOptions
{
    public double TokenExpiryInDays { get; set; }
    public string Key { get; set; }
    public string SecretKey { get; set; }
    public string Audience { get; set; }
    public string Issuer { get; set; }
    public string Endpoint { get; set; }
}