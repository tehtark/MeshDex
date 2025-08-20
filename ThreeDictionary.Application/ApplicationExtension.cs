using Microsoft.Extensions.DependencyInjection;
using ThreeDictionary.Application.Services;
using ThreeDictionary.Services;

namespace ThreeDictionary.Application;

public static class ApplicationExtension
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<LibraryService>();
        services.AddScoped<FileService>();
        return services;
    }
}