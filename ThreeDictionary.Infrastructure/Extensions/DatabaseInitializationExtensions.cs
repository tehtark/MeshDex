using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ThreeDictionary.Domain.Entities;
using ThreeDictionary.Infrastructure.Data;

namespace ThreeDictionary.Infrastructure.Extensions;

public static class DatabaseInitializationExtensions
{
    /// <summary>
    ///     Applies pending migrations and seeds baseline data in a safe, idempotent way.
    /// </summary>
    public static async Task InitialiseDatabaseAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        // Ensure the application data directory exists before EF tries to create/open the SQLite file
        var appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appdataDirectory = Path.Combine(appdataPath, "ThreeDictionary");
        if (!Directory.Exists(appdataDirectory)) Directory.CreateDirectory(appdataDirectory);

        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<ApplicationDbContext>();
        var config = sp.GetService<IConfiguration>();

        await db.Database.MigrateAsync(ct);

        // Seed default 'Uncategorised' category if missing
        var hasUncategorized = await db.LibraryCategories.AnyAsync(c => c.Name == "Uncategorized", ct);
        if (!hasUncategorized)
        {
            db.LibraryCategories.Add(new LibraryCategory { Name = "Uncategorized" });
            await db.SaveChangesAsync(ct);
        }

        // Seed default LibraryConfiguration with ID = 1 if missing
        var configuration = await db.LibraryConfigurations.FirstOrDefaultAsync(c => c.Id == 1, ct);
        if (configuration is null)
        {
            db.LibraryConfigurations.Add(new LibraryConfiguration
            {
                Id = 1,
                RootDirectory = string.Empty,
                Initialised = false
            });
            await db.SaveChangesAsync(ct);
        }

        // Seed Identity: Admin role and Admin user (idempotent)
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        const string adminRole = "Admin";
        if (!await roleManager.RoleExistsAsync(adminRole)) await roleManager.CreateAsync(new IdentityRole(adminRole));

        var userManager = sp.GetRequiredService<UserManager<User>>();
        var adminEmail = config?["Admin:Email"] ?? "admin@example.com";
        var adminPassword = config?["Admin:Password"] ?? "ChangeMe1!";
        var existingUser = await userManager.FindByEmailAsync(adminEmail);
        if (existingUser is null)
        {
            var adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (createResult.Succeeded) await userManager.AddToRoleAsync(adminUser, adminRole);
        }
        else
        {
            if (!await userManager.IsInRoleAsync(existingUser, adminRole)) await userManager.AddToRoleAsync(existingUser, adminRole);
        }
    }
}