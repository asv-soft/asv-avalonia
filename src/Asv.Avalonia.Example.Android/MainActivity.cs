using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia;
using Avalonia.Android;

namespace Asv.Avalonia.Example.Android;

[Activity(
    Label = "Asv.Avalonia.Example.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation
        | ConfigChanges.ScreenSize
        | ConfigChanges.UiMode
)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
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

        // this is required to use the AndroidHttpClientHandler in main thread
        StrictMode.SetThreadPolicy(new StrictMode.ThreadPolicy.Builder().PermitAll().Build());

        return base.CustomizeAppBuilder(builder).WithInterFont().UseR3();
    }
}
