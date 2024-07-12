using Jatoda.Application.Interfaces;
using Jatoda.Infrastructure.DataEFCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jatoda.Infrastructure.DataEFCore;

public static class DataEFCoreExtensions
{
    public static void RegisterDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlConnection");
        services.AddDbContext<JatodaContext>(options => options.UseNpgsql(connectionString));

        services.AddTransient<IRepositoryManager, RepositoryManager>();
    }
}