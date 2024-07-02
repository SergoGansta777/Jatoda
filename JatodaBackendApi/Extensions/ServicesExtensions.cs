using System.Reflection;
using System.Text;
using AspNetCoreRateLimit;
using JatodaBackendApi.Models.DBModels;
using JatodaBackendApi.Options;
using JatodaBackendApi.Providers;
using JatodaBackendApi.Providers.Interfaces;
using JatodaBackendApi.Repositories;
using JatodaBackendApi.Repositories.Interfaces;
using JatodaBackendApi.Services.AuthService;
using JatodaBackendApi.Services.AuthService.Interfaces;
using JatodaBackendApi.Services.CacheService;
using JatodaBackendApi.Services.CacheService.Interfaces;
using JatodaBackendApi.Services.CacheService.Repositories;
using JatodaBackendApi.Services.CacheService.Repositories.Interfaces;
using JatodaBackendApi.Services.JwtTokenService;
using JatodaBackendApi.Services.JwtTokenService.Interfaces;
using JatodaBackendApi.Services.MinIoService;
using JatodaBackendApi.Services.MinIoService.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minio;
using StackExchange.Redis;

namespace JatodaBackendApi.Extensions;

public static class ServicesExtensions
{
    public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        RegisterDbContext(services, configuration);
        RegisterCacheServices(services, configuration);
        RegisterRateLimiting(services, configuration);
        RegisterMinio(services, configuration);
        RegisterRepositories(services);
        RegisterScopedServices(services);
        RegisterOptions(services);
        RegisterAuthentication(services, configuration);
        RegisterSwagger(services, configuration);
        RegisterMiscellaneous(services);
    }

    private static void RegisterDbContext(IServiceCollection services, IConfiguration configuration)
    {
        var connectionStringSql = configuration.GetConnectionString("DbConnection");
        services.AddDbContext<JatodaContext>(options => options.UseNpgsql(connectionStringSql));
    }

    private static void RegisterCacheServices(IServiceCollection services, IConfiguration configuration)
    {
        var cacheConnectionString = configuration.GetConnectionString("CacheConnection");
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(cacheConnectionString));
        services.AddSingleton<ICacheRepository, CacheRepository>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
    }

    private static void RegisterRateLimiting(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));
    }

    private static void RegisterMinio(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MinioOptions>(configuration.GetSection("Minio"));
        services.AddSingleton<IMinioClient, MinioClient>();
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        services.AddScoped<IRepository<Todonote>, ToDoRepository>();
        services.AddScoped<IRepository<User>, UserRepository>();
        services.AddScoped<IRepository<Tag>, TagRepository>();
    }

    private static void RegisterScopedServices(IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ITodoProvider<Todonote>, TodoProvider>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserProvider<User>, UserProvider>();
        services.AddScoped<IMinioService, MinioService>();
        services.AddScoped<IFileProvider, FileProvider>();
    }

    private static void RegisterOptions(IServiceCollection services)
    {
        services.AddOptions();
        services.AddMemoryCache();
        services.AddDistributedMemoryCache();
        services.AddHttpContextAccessor();
        services.AddControllers();
        services.Configure<RouteOptions>(options => options.LowercaseUrls = true); 
    }

    private static void RegisterAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication(options =>
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
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!)
                    )
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

    private static void RegisterSwagger(IServiceCollection services, IConfiguration configuration)
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
            c.AddSecurityRequirement(new OpenApiSecurityRequirement {{securityScheme, Array.Empty<string>()}});
            
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });
    }

    private static void RegisterMiscellaneous(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAnyOrigin", builder =>
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });
    }
}