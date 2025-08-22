using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ThreeDictionary.Infrastructure.Data;

namespace ThreeDictionary.Infrastructure.Extensions;

public static class PersistenceExtensions
{
    /// <summary>
    ///     Registers the EF Core DbContext and configures the database provider.
    /// </summary>
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appDirectory = Path.Combine(appData, "ThreeDictionary");
        if (!Directory.Exists(appDirectory)) Directory.CreateDirectory(appDirectory);
        var databasePath = Path.Combine(appDirectory, "app.db");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(@$"DataSource={databasePath};Cache=Shared"));

        return services;
    }
}