using ThreeDictionary.Services;

namespace ThreeDictionary.Extensions;

public static class ThreeDictionaryExtension
{
    public static IServiceCollection AddThreeDictionary(this IServiceCollection services)
    {
        services.AddSingleton<LibraryService>();
        services.AddScoped<FileService>();
        return services;
    }
}