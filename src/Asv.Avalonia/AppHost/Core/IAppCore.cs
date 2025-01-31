using System.Composition.Hosting;
using Asv.Cfg;

namespace Asv.Avalonia;

public interface IAppCore
{
    public string AppName { get; set; }
    public string AppVersion { get; set; }
    public string CompanyName { get; set; }
    public string AvaloniaVersion { get; set; }
    public AppArgs Args { get; set; }
    public Func<IConfiguration, IAppInfo, string> UserDataFolder { get; set; }
    public string ProductTitle { get; set; }
    public string AppFolder { get; set; }
    public ILogService LogService { get; set; }
    public IConfiguration Configuration { get; set; }
    public ContainerConfiguration Services { get; set; }
    public Func<IAppInfo, string?> MutexName { get; set; }
    public Func<IAppInfo, string?> NamedPipe { get; set; }
}
