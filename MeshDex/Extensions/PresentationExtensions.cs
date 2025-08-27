using MeshDex.Components.Account;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;

namespace MeshDex.Extensions;

public static class PresentationExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddMudServices();
        services.AddRazorComponents().AddInteractiveServerComponents();

        services.AddCascadingAuthenticationState();
        services.AddScoped<IdentityUserAccessor>();
        services.AddScoped<IdentityRedirectManager>();
        services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
        return services;
    }
}