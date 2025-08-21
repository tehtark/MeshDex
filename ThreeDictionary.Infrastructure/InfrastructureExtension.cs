using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ThreeDictionary.Domain.Entities;
using ThreeDictionary.Infrastructure.Data;

namespace ThreeDictionary.Infrastructure;

public static class InfrastructureExtension
{
    public static async Task<IServiceCollection> AddInfrastructure(this IServiceCollection services)
    {
        // Ensure the application data directory exists.
        var appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appdataDirectory = Path.Combine(appdataPath, "ThreeDictionary");
        if (!Directory.Exists(appdataDirectory))
        {
            Directory.CreateDirectory(appdataDirectory);
        }
        var databasePath = Path.Combine(appdataDirectory, "app.db");
        
        // Configure the database context to use SQLite.
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(@$"DataSource={databasePath};Cache=Shared"));
        
        IServiceProvider serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        await db.Database.MigrateAsync();

        // Seed the database with default categories.
        var result = await db.LibraryCategories.AnyAsync(c => c.Name == "Uncategorized");
        if (!result)
        {
            var uncategorizedCategory = new LibraryCategory
            {
                Name = "Uncategorized",
            };
            db.LibraryCategories.Add(uncategorizedCategory);
            await db.SaveChangesAsync();
        }
 
        // Seed the database with a default configuration.
        var configuration = await db.LibraryConfigurations.FirstOrDefaultAsync(c => c.Id == 1);
        if (configuration != null)
        {
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
        return services;
    }
}