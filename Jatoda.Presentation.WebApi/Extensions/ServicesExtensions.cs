using System.Reflection;
using System.Text;
using AspNetCoreRateLimit;
using Jatoda.Application.Interfaces;
using Jatoda.Application.Service;
using Jatoda.Application.Service.Options;
using Jatoda.Domain.Data.DBModels;
using Jatoda.Infrastructure.CacheService;
using Jatoda.Infrastructure.CacheService.Interfaces;
using Jatoda.Infrastructure.CacheService.Repositories;
using Jatoda.Infrastructure.CacheService.Repositories.Interfaces;
using Jatoda.Infrastructure.DataEFCore;
using Jatoda.Infrastructure.DataEFCore.Repositories;
using Jatoda.Infrastructure.MinIoService;
using Jatoda.Infrastructure.MinIoService.Interfaces;
using Jatoda.Infrastructure.MinIoService.Options;
using Jatoda.Providers;
using Jatoda.Providers.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minio;
using StackExchange.Redis;

namespace Jatoda.Extensions;

public static class ServicesExtensions
{
    public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterAutoMapper();
        services.RegisterDatabase(configuration);
        services.RegisterCache(configuration);
        services.RegisterRateLimiting(configuration);
        services.RegisterMinio(configuration);
        services.RegisterRepositories();
        services.RegisterScopedServices();
        services.RegisterOptions(configuration);
        services.RegisterAuthentication(configuration);
        services.RegisterSwagger();
        services.RegisterMiscellaneous();
    }

    private static void RegisterDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlConnection");
        services.AddDbContext<JatodaContext>(options => options.UseNpgsql(connectionString));
    }

    private static void RegisterCache(this IServiceCollection services, IConfiguration configuration)
    {
        var cacheConnectionString = configuration.GetConnectionString("CacheConnection");
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(cacheConnectionString!));
        services.AddSingleton<ICacheRepository, CacheRepository>();
        services.AddSingleton<ICacheService, CacheService>();
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

    private static void RegisterMinio(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MinioOptions>(configuration.GetSection("Minio"));
        services.AddSingleton<IMinioClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MinioOptions>>().Value;
            return new MinioClient()
                .WithEndpoint(options.Endpoint)
                .WithCredentials(options.AccessKey, options.SecretKey)
                .Build();
        });
    }

    private static void RegisterRepositories(this IServiceCollection services)
    {
        services.AddScoped<IToDoRepository, ToDoRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITagRepository, TagRepository>();

        services.Decorate<IUserRepository, CachingUserRepository>();
        services.Decorate<IToDoRepository, CachingToDoRepository>();

        services.AddTransient<IRepositoryManager, RepositoryManager>();
    }

    private static void RegisterScopedServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IMinioService, MinioService>();

        services.AddScoped<ITodoProvider<Todo>, TodoProvider>();
        services.AddScoped<IAuthProvider, AuthProvider>();
        services.AddScoped<IUserProvider<User>, UserProvider>();
        services.AddScoped<IFileProvider, FileProvider>();
    }

    private static void RegisterOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();
        services.AddMemoryCache();
        services.AddDistributedMemoryCache();
        services.AddHttpContextAccessor();
        services.AddControllers();

        services.Configure<TokenOptions>(configuration.GetSection("jwt"));
        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
    }

    private static void RegisterAuthentication(this IServiceCollection services, IConfiguration configuration)
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

    private static void RegisterMiscellaneous(this IServiceCollection services)
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