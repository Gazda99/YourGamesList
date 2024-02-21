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
        app.UseMiddleware<ServerTimingMiddleware>();
        
        app.MapControllers();


        app.UseSwagger();
        app.UseSwaggerUI();
        
        // app.Lifetime.AddApplicationLifetimeActions();

        app.Run();
    }
}

//
// var builder = WebApplication.CreateBuilder(args);
//
// // Add services to the container.
// // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
//
// var app = builder.Build();
//
// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }
//
// app.UseHttpsRedirection();
//
// var summaries = new[]
// {
//     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };
//
// app.MapGet("/weatherforecast", () =>
//     {
//         var forecast = Enumerable.Range(1, 5).Select(index =>
//                 new WeatherForecast
//                 (
//                     DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//                     Random.Shared.Next(-20, 55),
//                     summaries[Random.Shared.Next(summaries.Length)]
//                 ))
//             .ToArray();
//         return forecast;
//     })
//     .WithName("GetWeatherForecast")
//     .WithOpenApi();
//
// app.Run();
//
// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int) (TemperatureC / 0.5556);
// }