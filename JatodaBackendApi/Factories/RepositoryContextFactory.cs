using JatodaBackendApi.Models.DBModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JatodaBackendApi.Factories;

public class RepositoryContextFactory : IDesignTimeDbContextFactory<JatodaContext>
{
    public JatodaContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var builder = new DbContextOptionsBuilder().UseNpgsql(configuration.GetConnectionString("SqlConnection"));

        return new JatodaContext(builder.Options);
    }
}