using System.Composition.Hosting;
using Asv.Cfg;

namespace Asv.Avalonia;

public class AppCore : IAppCore
{
    private const string ZeroVersion = "0.0.0";

    public string AppName { get; set; } = string.Empty;
    public string AppVersion { get; set; } = ZeroVersion;
    public string CompanyName { get; set; } = string.Empty;
    public string AvaloniaVersion { get; set; } = ZeroVersion;
    public AppArgs Args { get; set; } = new([]);
    public required Func<IConfiguration, IAppInfo, string> UserDataFolder { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public required string AppFolder { get; set; }
    public ILogService LogService { get; set; } = NullLogService.Instance;
    public required IConfiguration Configuration { get; set; }
    public required ContainerConfiguration Services { get; set; }
    public Func<IAppInfo, string?> MutexName { get; set; } = _ => null;
    public Func<IAppInfo, string?> NamedPipe { get; set; } = _ => null;
}
