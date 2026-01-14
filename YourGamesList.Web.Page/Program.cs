using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using YourGamesList.Common.Logging;
using YourGamesList.Web.Page.AppBuilders;
using YourGamesList.Web.Page.Components;

namespace YourGamesList.Web.Page;

[ExcludeFromCodeCoverage]
public class Program
{
    public static void Main(string[] args)
    {
        var app = AppBuilder.GetApp(args);

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Lifetime.AddApplicationLifetimeActions(app.Services);

        app.Run();
    }
}