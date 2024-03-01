using Microsoft.AspNetCore.Builder;
using Serilog;
using YourGamesList.Common.Middleware;

namespace YourGamesList.IgdbScraper;

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

        app.MapControllers();


        app.Run();
    }
}