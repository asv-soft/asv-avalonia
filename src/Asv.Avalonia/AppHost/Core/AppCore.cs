using Asv.Cfg;

namespace Asv.Avalonia;

public class AppCore : IAppCore
{
    public required string AppFolder { get; set; }
    public required AppInfo AppInfo { get; set; }
    public required Func<IConfiguration, IAppInfo, string> UserDataFolder { get; set; } = null!;

    public AppArgs Args { get; set; } = null!;
    public IConfiguration Configuration { get; set; } = null!;
    public ILogService LogService { get; set; } = NullLogService.Instance;
    public Func<IAppInfo, string?> MutexName { get; set; } = _ => null;
    public Func<IAppInfo, string?> NamedPipe { get; set; } = _ => null;
}
