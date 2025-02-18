using System.Composition.Hosting;
using Asv.Cfg;

namespace Asv.Avalonia;

public interface IAppCore
{
    // Required
    public ContainerConfiguration Services { get; internal set; }
    public string AppFolder { get; internal set; }
    public AppInfo AppInfo { get; internal set; }
    public Func<IConfiguration, IAppInfo, string> UserDataFolder { get; internal set; }

    public AppArgs Args { get; internal set; }
    public IConfiguration Configuration { get; internal set; }
    public ILogService LogService { get; internal set; }
    public Func<IAppInfo, string?> MutexName { get; internal set; }
    public Func<IAppInfo, string?> NamedPipe { get; internal set; }
}
