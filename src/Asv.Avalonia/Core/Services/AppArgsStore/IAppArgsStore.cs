using R3;

namespace Asv.Avalonia;

public interface IAppArgsStore
{
    ReadOnlyReactiveProperty<IAppArgs> Args { get; }
    void Set(IAppArgs args);
}
