using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public class AsvApplication : Application
{
    protected AsvApplication()
    {
        DataTemplates.Add(AppHost.Instance.Services.GetRequiredService<ViewLocator>());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var shellHost = AppHost.Instance.Services.GetRequiredService<IShellHost>();
        var shell = AppHost.Instance.Services.GetRequiredService<IShell>();

        if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (desktop.MainWindow is TopLevel topLevel)
            {
                shellHost.Init(shell, topLevel);
            }
            desktop.Exit += (_, _) => Dispose();
        }
        else if (Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            if (singleViewPlatform.MainView is TopLevel topLevel)
            {
                shellHost.Init(shell, topLevel);
            }
        }
        else
        {
            if (!Design.IsDesignMode)
            {
                throw new Exception("Unknown platform");
            }
        }

        base.OnFrameworkInitializationCompleted();
#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
