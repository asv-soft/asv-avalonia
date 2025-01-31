using System.Composition.Hosting;
using Asv.Cfg;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using R3;
using InMemoryConfiguration = Asv.Cfg.InMemoryConfiguration;

namespace Asv.Avalonia;

public class NullAppHost : IAppHost
{
    public static IAppHost Instance { get; } = new NullAppHost();

    private NullAppHost()
    {
        Args = new ReactiveProperty<AppArgs>(new AppArgs([]));
        AppInfo = new AppInfo
        {
            Title = "Design",
            Name = "Design",
            Version = "1.0.0",
            CompanyName = "Design",
            AvaloniaVersion = "1.0.0",
        };
        AppPath = new AppPath { UserDataFolder = ".", AppFolder = "." };
    }

    public ReadOnlyReactiveProperty<AppArgs> Args { get; }
    public IAppInfo AppInfo { get; }
    public IAppPath AppPath { get; }

    public void RegisterServices(ContainerConfiguration containerCfg)
    {
        containerCfg
            .WithExport(AppInfo)
            .WithExport(AppPath)
            .WithExport(NullLogService.Instance)
            .WithExport<ILoggerFactory>(NullLogService.Instance)
            .WithExport(new InMemoryConfiguration())
            .WithExport(Args)
            .WithExport(this);
    }

    public void HandleApplicationCrash(Exception exception)
    {
        // do nothing
    }

    public bool AllowOnlyOneInstance { get; } = false;
    public bool IsFirstInstance { get; } = true;

    public void Dispose()
    {
        Args.Dispose();
    }
}
