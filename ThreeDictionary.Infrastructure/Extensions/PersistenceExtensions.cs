using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ThreeDictionary.Infrastructure.Data;

namespace ThreeDictionary.Infrastructure.Extensions;

public static class PersistenceExtensions
{
    /// <summary>
    /// Registers the EF Core DbContext and configures the database provider.
    /// </summary>
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        // Compute the database file path under the user's roaming AppData directory.
        var appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appdataDirectory = Path.Combine(appdataPath, "ThreeDictionary");
        var databasePath = Path.Combine(appdataDirectory, "app.db");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(@$"DataSource={databasePath};Cache=Shared"));

        return services;
    }
}
