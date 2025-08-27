using MeshDex.Domain.Entities;
using MeshDex.Infrastructure.Data;
using MeshDex.Infrastructure.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace MeshDex.Infrastructure.Extensions;

public static class IdentityExtensions
{
    public static IServiceCollection AddIdentityAndAuth(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddIdentityCore<User>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                // Bind more options if you wish:
                // config.GetSection("Identity:Password").Bind(options.Password);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddSingleton<IEmailSender<User>, IdentityNoOpEmailSender>();

        return services;
    }
}