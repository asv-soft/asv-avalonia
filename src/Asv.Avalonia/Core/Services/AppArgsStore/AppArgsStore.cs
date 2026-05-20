using Asv.Common;
using R3;

namespace Asv.Avalonia;

public sealed class AppArgsStore : AsyncDisposableOnceBag, IAppArgsStore
{
    private readonly SynchronizedReactiveProperty<IAppArgs> _args;

    public AppArgsStore()
    {
        _args = new SynchronizedReactiveProperty<IAppArgs>(
            new AppArgs(Environment.GetCommandLineArgs())
        ).AddTo(ref DisposableBag);
        Args = _args.ToReadOnlyReactiveProperty().AddTo(ref DisposableBag);
    }

    public ReadOnlyReactiveProperty<IAppArgs> Args { get; }

    public void Set(IAppArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);
        _args.OnNext(args);
    }
}
