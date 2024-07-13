using System.Text;
using Jatoda.Application.Interfaces;
using Jatoda.Domain.Core.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Jatoda.Application.Service;

public static class ServicesExtensions
{
    public static void RegisterInternalServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailConfirmationOptions>(configuration.GetSection("email-confirmation"));
        services.Configure<TokenOptions>(configuration.GetSection("jwt"));

        services.AddScoped<IEmailConfirmationService, EmailConfirmationService>();
        services.AddSingleton<ITokenService, TokenService>();
    }

    public static void RegisterJwtAuthenticationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSecretKey = Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!);

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(jwtSecretKey)
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["jwt"];
                        return Task.CompletedTask;
                    }
                };
            });
    }
}