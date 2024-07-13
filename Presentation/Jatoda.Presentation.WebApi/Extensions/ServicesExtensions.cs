using System.Reflection;
using AspNetCoreRateLimit;
using Jatoda.Application.Core;
using Jatoda.Application.Services;
using Jatoda.Domain.Data.DBModels;
using Jatoda.Infrastructure.CacheService;
using Jatoda.Infrastructure.EFCore;
using Jatoda.Infrastructure.MinIoService;
using Jatoda.Providers;
using Jatoda.Providers.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace Jatoda.Extensions;

public static class ServicesExtensions
{
    public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterOptions();

        services.RegisterDatabase(configuration);
        services.RegisterCacheService(configuration);
        services.RegisterRequestCache();

        services.RegisterRateLimiting(configuration);

        services.RegisterInternalServices(configuration);
        services.RegisterProviders();
        services.RegisterMinio(configuration);
        services.RegisterJwtAuthenticationOptions(configuration);

        services.RegisterAutoMapper();
        services.RegisterSwagger();
    }


    private static void RegisterRequestCache(this IServiceCollection services)
    {
        services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
    }

    private static void RegisterRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));
    }

    private static void RegisterProviders(this IServiceCollection services)
    {
        services.AddScoped<ITodoProvider<Todo>, TodoProvider>();
        services.AddScoped<IAuthProvider, AuthProvider>();
        services.AddScoped<IUserProvider<User>, UserProvider>();
        services.AddScoped<IFileProvider, FileProvider>();
    }

    private static void RegisterOptions(this IServiceCollection services)
    {
        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

        services.AddOptions();
        services.AddMemoryCache();
        services.AddDistributedMemoryCache();
        services.AddHttpContextAccessor();
        services.AddControllers();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAnyOrigin", builder =>
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });
    }

    private static void RegisterSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo {Title = "Just Another ToDo App Api", Version = "v1"});

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "JWT Authentication",
                Description = "Enter JWT Bearer token **_only_**",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer", // must be lower case
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {securityScheme, Array.Empty<string>()}
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });
    }
}