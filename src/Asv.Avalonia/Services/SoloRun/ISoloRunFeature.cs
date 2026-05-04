using Microsoft.Extensions.Hosting;
using R3;

namespace Asv.Avalonia;

public interface ISoloRunFeature : IHostedService, IDisposable
{
    bool IsFirstInstance { get; }
    ReadOnlyReactiveProperty<IAppArgs> Args { get; }
}
