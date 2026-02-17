using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Avalonia.Example;

public class App : Application, IShellHost
{
    private readonly Subject<IShell> _onShellLoaded = new();

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
        Shell = AppHost.Instance.GetService<IShell>();

        if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (desktop.MainWindow is TopLevel topLevel)
            {
                TopLevel = topLevel;
            }
        }
        else if (Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            if (singleViewPlatform.MainView is TopLevel topLevel)
            {
                TopLevel = topLevel;
            }
        }
        else
        {
            throw new Exception("Unknown platform");
        }

        base.OnFrameworkInitializationCompleted();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    public void Dispose()
    {
        Shell.Dispose();
        AppHost.Instance.Dispose();
        _onShellLoaded.Dispose();
        GC.SuppressFinalize(this);
    }

    public IShell Shell
    {
        get;
        private set
        {
            field = value;
            _onShellLoaded.OnNext(value);
        }
    }

    public Observable<IShell> OnShellLoaded => _onShellLoaded;
    public TopLevel TopLevel { get; private set; }
}
