using Microsoft.Extensions.DependencyInjection;

namespace ThreeDictionary.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddIdentityAndAuth();
        services.AddPersistence();
        return services;
    }
}