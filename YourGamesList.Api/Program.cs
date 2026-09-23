using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using YourGamesList.Api.Middlewares;

namespace YourGamesList.Api;

[ExcludeFromCodeCoverage]
public class Program
{
    public static void Main(string[] args)
    {
        var app = AppBuilder.GetApp(args);

        app.UseMiddleware<CorrelationIdMiddleware>();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseMiddleware<UserInformationLoggingMiddleware>();
        app.UseAuthorization();
        app.UseMiddleware<ExceptionMiddleware>();

        app.MapControllers();

        app.Run();
    }
}