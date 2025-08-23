using Serilog;
using Serilog.Events;

namespace ThreeDictionary.Extensions;

public static class LoggingExtensions
{
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder app)
    {
        app.Host.UseSerilog((_, logger) =>
        {
#if DEBUG
            logger.MinimumLevel.Is(LogEventLevel.Debug)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning) // Filter specific namespace
                .MinimumLevel.Override("MudBlazor", LogEventLevel.Warning) // Filter specific namespace
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Debug) // Filter specific namespace
                .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "log.json"), rollingInterval: RollingInterval.Day, shared: true)
                .WriteTo.Console();
#else
            logger.MinimumLevel.Is(LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning) // Filter specific namespace
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning) // Filter specific namespace
                .MinimumLevel.Override("MudBlazor", LogEventLevel.Warning) // Filter specific namespace
                .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "log.json"), rollingInterval: RollingInterval.Day, shared: true)
                .WriteTo.Console();
#endif
        });
        Log.Debug("Logger: Initialised.");
        return app;
    }
    
    public static IApplicationBuilder UseSerilogMiddleware(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.GetLevel = (httpContext, elapsed, ex) =>
            {
                var path = httpContext.Request.Path;
                if (ex != null || httpContext.Response.StatusCode >= 500) return LogEventLevel.Error;
                if (path.StartsWithSegments("/_framework")
                    || path.StartsWithSegments("/_blazor")
                    || path.StartsWithSegments("/css")
                    || path.StartsWithSegments("/js")
                    || path.StartsWithSegments("/images")
                    || path.StartsWithSegments("/favicon.ico")
                    || path.StartsWithSegments("/files"))
                {
                    return LogEventLevel.Verbose; // suppressed in Debug (min=Debug) and Release (min=Information)
                }
                return elapsed > 1000 ? LogEventLevel.Warning : LogEventLevel.Debug;
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
        return app;
    }
}