using MeshDex.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MeshDex.Application;

public static class ApplicationExtension
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(ApplicationExtension).Assembly);
        });

        services.AddScoped<LibraryConfigurationService>();
        services.AddScoped<LibraryCategoryService>();
        services.AddScoped<LibraryModelService>();
        return services;
    }
}