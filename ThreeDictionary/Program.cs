using Microsoft.AspNetCore.Identity;
using Serilog;
using ThreeDictionary.Application;
using ThreeDictionary.Components;
using ThreeDictionary.Domain.Entities;
using ThreeDictionary.Extensions;
using ThreeDictionary.Infrastructure.Extensions;

namespace ThreeDictionary;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.AddSerilogLogging();
        builder.Services.AddApplication();
        await builder.Services.AddInfrastructure();
        builder.Services.AddPresentation();
        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error", true);
            // https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseAntiforgery();
        app.MapStaticAssets();
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
        app.MapAdditionalIdentityEndpoints();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            await CreateRolesAsync(services);
            await CreateAdminUserAsync(services);
        }

        await app.RunAsync();
    }
    private static async Task CreateRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = ["Admin"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                // Create the roles and seed them to the database
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                {
                    Log.Error("Failed to create role: {Role}", role);
                }
                else
                {
                    Log.Information("Role created: {Role}", role);
                }
            }
            else
            {
                Log.Information("Role already exists: {Role}", role);
            }
        }
    }

    private static async Task CreateAdminUserAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var email = "admin@example.com";
        var password = "ChangeMe1!";

        var user = await userManager.FindByNameAsync(email);
        if (user == null)
        {
            user = new User { UserName = email, Email = email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}