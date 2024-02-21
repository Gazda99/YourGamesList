using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace YourGamesList.Common.Services.Serilog;

public static class SerilogInstaller
{
    public static ConfigureHostBuilder AddLogger(this ConfigureHostBuilder host, IConfiguration configuration)
    {
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration);
        global::Serilog.Log.Logger = logger.CreateLogger();
        host.UseSerilog();

        return host;
    }
}