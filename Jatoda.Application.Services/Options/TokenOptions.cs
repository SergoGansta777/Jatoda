namespace Jatoda.Application.Service.Options;

public class TokenOptions
{
    public double TokenExpiry { get; set; }
    public string Key { get; set; }
    public string SecretKey { get; set; }
    public string Audience { get; set; }
    public string Issuer { get; set; }
    public string Endpoint { get; set; }
}