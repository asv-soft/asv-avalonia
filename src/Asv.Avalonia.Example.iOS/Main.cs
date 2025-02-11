using System;
using UIKit;

namespace Asv.Avalonia.Example.iOS;

public class Application
{
    // This is the main entry point of the application.
    static void Main(string[] args)
    {
        var builder = AppHost.CreateBuilder();

        builder.UseJsonConfig("config.json", true, TimeSpan.FromMilliseconds(500));
        builder.SetAppInfoFrom(typeof(App).Assembly);
        builder.UseLogging(options =>
        {
            options.WithLogMinimumLevel<AppHostConfig>(cfg => cfg.LogMinLevel);
            options.WithJsonLogFolder<AppHostConfig>("logs", cfg => cfg.RollingSizeKb);
            options.WithLogToConsole();
        });
        builder.SetArguments(args);

        using var host = builder.Build();
        try
        {
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            UIApplication.Main(args, null, typeof(AppDelegate));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            host.HandleApplicationCrash(e);
        }
    }
}
