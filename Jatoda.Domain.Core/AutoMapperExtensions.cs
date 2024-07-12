using Jatoda.Domain.Core.Mappers;
using Microsoft.Extensions.DependencyInjection;

namespace Jatoda.Domain.Core;

public static class AutoMapperExtensions
{
    public static void RegisterAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(srv => { srv.AddProfile<TodonoteToTodonoteViewModel>(); });
    }
}