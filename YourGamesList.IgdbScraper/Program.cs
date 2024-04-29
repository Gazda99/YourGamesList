using Microsoft.AspNetCore.Builder;

namespace YourGamesList.IgdbScraper;

public static class Program
{
    public static void Main(string[] args)
    {
        var app = AppBuilder.GetApp(args);

        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.MapControllers();


        app.Run();
    }
}