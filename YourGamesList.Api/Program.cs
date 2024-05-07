using Microsoft.AspNetCore.Builder;
using YourGamesList.Common.Middlewares;

namespace YourGamesList.Api;

public static class Program
{
    public static void Main(string[] args)
    {
        var app = AppBuilder.GetApp(args);

        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.UseServerTiming();
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<ServerTimingsMiddleware>();

        app.MapControllers();


        app.UseSwagger();
        app.UseSwaggerUI();

        // app.Lifetime.AddApplicationLifetimeActions();

        app.Run();
    }
}