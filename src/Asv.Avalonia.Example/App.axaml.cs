using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Avalonia.Example;

public class App : Application
{
    public App()
    {
        DataTemplates.Add(AppHost.Instance.Services.GetRequiredService<ViewLocator>());
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var shellHost = AppHost.Instance.Services.GetRequiredService<IShellHost>();
        var shell = AppHost.Instance.Services.GetRequiredService<IShell>();

        if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (desktop.MainWindow is TopLevel topLevel)
            {
                shellHost.TopLevel = topLevel;
                shellHost.Shell = shell;
            }
        }
        else if (Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            if (singleViewPlatform.MainView is TopLevel topLevel)
            {
                shellHost.TopLevel = topLevel;
                shellHost.Shell = shell;
            }
        }
        else
        {
            if (Design.IsDesignMode == false)
            {
                throw new Exception("Unknown platform");
            }
        }

        base.OnFrameworkInitializationCompleted();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    public void Dispose()
    {
        AppHost.Instance.Dispose();
        GC.SuppressFinalize(this);
    }
}
