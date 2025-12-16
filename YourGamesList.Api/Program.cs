using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using YourGamesList.Api.AppBuilders;
using YourGamesList.Api.Middlewares;
using YourGamesList.Common.Logging;

namespace YourGamesList.Api;

[ExcludeFromCodeCoverage]
public class Program
{
    public static void Main(string[] args)
    {
        var app = AppBuilder.GetApp(args);
        app.UseRouting();

        app.UseMiddleware<JwtUserInformationMiddleware>();
        app.UseMiddleware<CorrelationIdMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<ExceptionMiddleware>();

        app.MapControllers();

        app.UseOutputCache();

        app.Lifetime.AddApplicationLifetimeActions(app.Services);

        app.Run();
    }
}