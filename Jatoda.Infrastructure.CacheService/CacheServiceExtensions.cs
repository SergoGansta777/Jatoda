using Jatoda.Infrastructure.CacheService.Interfaces;
using Jatoda.Infrastructure.CacheService.Repositories;
using Jatoda.Infrastructure.CacheService.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Jatoda.Infrastructure.CacheService;

public static class CacheServiceExtensions
{
    public static void RegisterCacheService(this IServiceCollection services, IConfiguration configuration)
    {
        var cacheConnectionString = configuration.GetConnectionString("CacheConnection");

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(cacheConnectionString!));
        services.AddSingleton<ICacheRepository, CacheRepository>();
        services.AddSingleton<ICacheService, CacheService>();
    }
}