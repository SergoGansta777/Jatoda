using System.Text;
using AspNetCoreRateLimit;
using JatodaBackendApi.Extensions;
using JatodaBackendApi.Mappers;
using JatodaBackendApi.Models.DBModels;
using JatodaBackendApi.Options;
using JatodaBackendApi.Providers;
using JatodaBackendApi.Providers.Interfaces;
using JatodaBackendApi.Repositories;
using JatodaBackendApi.Repositories.Interfaces;
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

namespace JatodaBackendApi;

public class Startup
{
    public static void ConfigureServices(
        HostBuilderContext hostContext,
        IServiceCollection services
    )
    {
        var configuration = hostContext.Configuration;

        services.AddLogging();
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo {Title = "Your API", Version = "v1"});

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
            c.AddSecurityRequirement(
                new OpenApiSecurityRequirement {{securityScheme, Array.Empty<string>()}}
            );
        });

        var connectionStringSql = configuration.GetConnectionString("DbConnection");
        services.AddDbContext<JatodaContext>(options => { options.UseNpgsql(connectionStringSql); });
        var cacheConnectionString = configuration.GetConnectionString("CacheConnection");
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(cacheConnectionString!)
        );

        services.AddOptions();
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));
        services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
        
        services.AddDistributedMemoryCache();
        services.AddSingleton<ICacheRepository, CacheRepository>();
        services.AddSingleton<ICacheService, CacheService>();

        services.AddScoped<IRepository<Todonote>, ToDoRepository>();
        services.AddScoped<IRepository<User>, UserRepository>();
        services.AddScoped<IRepository<Tag>, TagRepository>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ITodoProvider<Todonote>, TodoProvider>();
        services.AddScoped<IUserProvider<User>, UserProvider>();
        services.AddScoped<IMinioService, MinioService>();
        services.AddScoped<IFileProvider, FileProvider>();

        services.AddSingleton<IMinioClient, MinioClient>();

        services.Configure<MinioOptions>(configuration.GetSection("Minio"));

        services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowAnyOrigin",
                builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
            );
        });

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

        services.AddAutoMapper(typeof(TodonoteToTodonoteViewModel));
    }

    public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        var logger = app.ApplicationServices.GetRequiredService<ILogger<Startup>>();
        app.ConfigureExceptionHandler(logger);

        app.UseHttpsRedirection();
        app.UseIpRateLimiting();
        app.UseRouting();
        app.UseAuthorization();
        app.UseAuthentication();
        app.UseCors(options => options
            .WithOrigins("http://localhost:3000", "http://localhost:8080", "http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
        );

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}