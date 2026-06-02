using Asv.Common;
using R3;

namespace Asv.Avalonia;

public sealed class NullAppArgsStore : IAppArgsStore
{
    public static IAppArgsStore Instance { get; } = new NullAppArgsStore();

    public NullAppArgsStore()
    {
        Args = new ReactiveProperty<IAppArgs>(new AppArgs([]));
    }

    public ReadOnlyReactiveProperty<IAppArgs> Args { get; }

    public void Set(IAppArgs args)
    {
        return;
    }
}
