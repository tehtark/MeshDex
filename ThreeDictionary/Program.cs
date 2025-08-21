using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using MudBlazor.Services;
using Serilog;
using Serilog.Events;
using ThreeDictionary.Application;
using ThreeDictionary.Components;
using ThreeDictionary.Components.Account;
using ThreeDictionary.Domain.Entities;
using ThreeDictionary.Infrastructure;
using ThreeDictionary.Infrastructure.Data;

namespace ThreeDictionary;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        InitialiseLogger(builder);

        await builder.Services.AddInfrastructure();
        builder.Services.AddApplication();

        builder.Services.AddMudServices();

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<IdentityUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();
        builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddIdentityCore<User>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        builder.Services.AddSingleton<IEmailSender<User>, IdentityNoOpEmailSender>();


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
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.MapAdditionalIdentityEndpoints();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            await CreateRolesAsync(services);
            await CreateAdminUserAsync(services);
        }

        await app.RunAsync();
    }

    private static void InitialiseLogger(WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((_, logger) =>
        {
#if DEBUG
            logger.MinimumLevel.Is(LogEventLevel.Debug)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning) // Filter specific namespace
                .MinimumLevel.Override("MudBlazor", LogEventLevel.Warning) // Filter specific namespace
                .WriteTo.File(AppDomain.CurrentDomain.BaseDirectory + "/logs/log.json", rollingInterval: RollingInterval.Day, shared: true)
                .WriteTo.Console();
#else
            logger.MinimumLevel.Is(LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning) // Filter specific namespace
                .MinimumLevel.Override("MudBlazor", LogEventLevel.Warning) // Filter specific namespace
                .WriteTo.File(AppDomain.CurrentDomain.BaseDirectory + "/logs/log.json", rollingInterval: RollingInterval.Day, shared: true)
                .WriteTo.Console();
#endif
        });
        Log.Debug("Logger: Initialised.");
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