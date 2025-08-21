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
        return app;
    }
}