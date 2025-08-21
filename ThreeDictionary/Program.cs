using Serilog;
using Serilog.AspNetCore;
using ThreeDictionary.Application;
using ThreeDictionary.Components;
using ThreeDictionary.Extensions;
using ThreeDictionary.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogLogging();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddPresentation();

var app = builder.Build();

await app.Services.InitialiseDatabaseAsync();

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
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        var path = httpContext.Request.Path;
        if (ex != null || httpContext.Response.StatusCode >= 500) return Serilog.Events.LogEventLevel.Error;
        if (path.StartsWithSegments("/_framework")
            || path.StartsWithSegments("/_blazor")
            || path.StartsWithSegments("/css")
            || path.StartsWithSegments("/js")
            || path.StartsWithSegments("/images")
            || path.StartsWithSegments("/favicon.ico")
            || path.StartsWithSegments("/files"))
        {
            return Serilog.Events.LogEventLevel.Verbose; // suppressed in Debug (min=Debug) and Release (min=Information)
        }
        return elapsed > 1000 ? Serilog.Events.LogEventLevel.Warning : Serilog.Events.LogEventLevel.Information;
    };
    options.EnrichDiagnosticContext = (diag, httpContext) =>
    {
        var user = httpContext.User?.Identity?.IsAuthenticated == true ? httpContext.User.Identity?.Name : null;
        diag.Set("UserName", user);
        diag.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
        diag.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        diag.Set("RequestId", httpContext.TraceIdentifier);
    };
});
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapAdditionalIdentityEndpoints();


await app.RunAsync();