using System.Composition.Hosting;
using R3;

namespace Asv.Avalonia;

public interface IAppHost : IDisposable
{
    ReadOnlyReactiveProperty<AppArgs> Args { get; }
    IAppInfo AppInfo { get; }
    IAppPath AppPath { get; }
    void RegisterServices(ContainerConfiguration containerCfg);
    void HandleApplicationCrash(Exception exception);
    bool AllowOnlyOneInstance { get; }
    bool IsFirstInstance { get; }
}
