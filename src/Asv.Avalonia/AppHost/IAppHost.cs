using System.Collections.Concurrent;
using System.Composition.Hosting;
using Asv.Cfg;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public interface IAppHost : IDisposable
{
    ReadOnlyReactiveProperty<AppArgs> Args { get; }
    IAppInfo AppInfo { get; }
    IAppPath AppPath { get; }
    IConfiguration Configuration { get; }
    ILogService Logs { get; }
    IDictionary<Type, IBuilderOptions> BuilderOptions { get; }
    void HandleApplicationCrash(Exception exception);
    bool AllowOnlyOneInstance { get; }
    bool IsFirstInstance { get; }
}
