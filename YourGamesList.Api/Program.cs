using Microsoft.AspNetCore.Builder;
using Serilog;
using YourGamesList.Common.Middleware;

namespace YourGamesList.Api;

public static class Program
{
    public static void Main(string[] args)
    {
        var app = AppBuilder.GetApp(args);
        app.UseSerilogRequestLogging(options =>
            options.IncludeQueryInRequestPath = false
        );

        app.UseHttpsRedirection();
        app.UseAuthorization();
        
        app.UseServerTiming();
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<ServerTimingMiddleware>();
        
        app.MapControllers();


        app.UseSwagger();
        app.UseSwaggerUI();
        
        // app.Lifetime.AddApplicationLifetimeActions();

        app.Run();
    }
}