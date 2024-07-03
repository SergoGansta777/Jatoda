using JatodaBackendApi.Mappers;

namespace JatodaBackendApi.Extensions;

public static class AutoMapperExtensions
{
    internal static void RegisterAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(srv => { srv.AddProfile<TodonoteToTodonoteViewModel>(); });
    }
}