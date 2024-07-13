using Jatoda.Application.Core.Mappers;
using Microsoft.Extensions.DependencyInjection;

namespace Jatoda.Application.Core;

public static class AutoMapperExtensions
{
    public static void RegisterAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(srv => { srv.AddProfile<TodoToTodoDto>(); });
    }
}