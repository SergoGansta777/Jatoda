using Jatoda.Domain.Core.Options;
using Jatoda.Infrastructure.MinIoService.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Minio;

namespace Jatoda.Infrastructure.MinIoService;

public static class MinIoServiceExtensions
{
    public static void RegisterMinio(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MinioOptions>(configuration.GetSection("minio"));

        services.AddSingleton<IMinioClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MinioOptions>>().Value;
            return new MinioClient()
                .WithEndpoint(options.Endpoint)
                .WithCredentials(options.AccessKey, options.SecretKey)
                .Build();
        });

        services.AddScoped<IMinioService, MinioService>();
    }
}