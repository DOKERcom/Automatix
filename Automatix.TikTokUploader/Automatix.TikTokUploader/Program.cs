using Automatix.TikTokUploader.Models;
using Microsoft.Extensions.Configuration;

namespace Automatix.TikTokUploader;

public class Program
{
    static void Main(string[] args)
    {
        var cfg = GetAppConfiguration();

        var startup = new Startup();

        startup.Run(cfg);

        Console.ReadLine();
    }

    private static AppConfigModel? GetAppConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(AppContext.BaseDirectory))
            .AddJsonFile($"appconfig.json", optional: false, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        return configuration.GetRequiredSection("Configuration").Get<AppConfigModel>();
    }
}