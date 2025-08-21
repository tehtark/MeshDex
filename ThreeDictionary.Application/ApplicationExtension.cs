using Microsoft.Extensions.DependencyInjection;
using ThreeDictionary.Application.Services;

namespace ThreeDictionary.Application;

public static class ApplicationExtension
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<LibraryConfigurationService>();
        services.AddScoped<LibraryCategoryService>();
        return services;
    }
}