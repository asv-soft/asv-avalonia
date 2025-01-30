using Asv.Cfg;
using R3;

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
        Configuration = new InMemoryConfiguration();
        Logs = NullLogService.Instance;
    }

    public ReadOnlyReactiveProperty<AppArgs> Args { get; }
    public IAppInfo AppInfo { get; }
    public IAppPath AppPath { get; }
    public IConfiguration Configuration { get; }
    public ILogService Logs { get; }

    public void HandleApplicationCrash(Exception exception)
    {
        // do nothing
    }

    public bool AllowOnlyOneInstance { get; } = false;
    public bool IsFirstInstance { get; } = true;

    public void Dispose()
    {
        Args.Dispose();
        Configuration.Dispose();
        Logs.Dispose();
    }
}
