using R3;

namespace Asv.Avalonia;

public interface ISoloRunFeature : IDisposable
{
    bool IsFirstInstance { get; }
    ReadOnlyReactiveProperty<AppArgs> Args { get; }
}
