using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ThreeDictionary.Domain.Entities;
using ThreeDictionary.Infrastructure.Data;

namespace ThreeDictionary.Infrastructure;

public static class InfrastructureExtension
{
    public static async Task<IServiceCollection> AddInfrastructure(this IServiceCollection services)
    {
        var appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appdataDirectory = Path.Combine(appdataPath, "ThreeDictionary");
        if (!Directory.Exists(appdataDirectory))
        {
            Directory.CreateDirectory(appdataDirectory);
        }
        var databasePath = Path.Combine(appdataDirectory, "app.db");
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(@$"DataSource={databasePath};Cache=Shared"));
        
        IServiceProvider serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        await db.Database.MigrateAsync();

        // Check if the configuration already exists
        var configuration = await db.LibraryConfigurations.FirstOrDefaultAsync(c => c.Id == 1);

        // If it exists, return the services
        if (configuration != null)
        {
            services.AddSingleton(configuration);;
            return services;
        }
        
        configuration = new LibraryConfiguration
        {
            Id = 1,
            RootDirectory = "",
            Initialised = false,

        };
        db.LibraryConfigurations.Add(configuration);
        await db.SaveChangesAsync();
        services.AddSingleton(configuration);;
        return services;
    }
}