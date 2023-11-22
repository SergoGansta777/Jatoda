using JatodaBackendApi.Services.LoggerService.Interfaces;

namespace JatodaBackendApi.Services.LoggerService;

public static class LoggerServiceExtension
{
    public static void ConfigureLoggerService(this IServiceCollection services)
    {
        services.AddSingleton<ILoggerManager, LoggerManager>();
    }
}