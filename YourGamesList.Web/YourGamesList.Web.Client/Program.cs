using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

namespace YourGamesList.Web.Client;

[ExcludeFromCodeCoverage]
class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.Services.AddMudServices();

        builder.Services.AddFluxor(x =>
            x.ScanAssemblies(typeof(YourGamesList.Web.Store.Marker).Assembly)
        );


        await builder.Build().RunAsync();
    }
}