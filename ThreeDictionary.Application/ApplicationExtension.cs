using Microsoft.Extensions.DependencyInjection;
using ThreeDictionary.Application.Services;

namespace ThreeDictionary.Application;

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