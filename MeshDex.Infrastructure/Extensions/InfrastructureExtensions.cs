using Microsoft.Extensions.DependencyInjection;

namespace MeshDex.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddIdentityAndAuth();
        services.AddPersistence();
        return services;
    }
}