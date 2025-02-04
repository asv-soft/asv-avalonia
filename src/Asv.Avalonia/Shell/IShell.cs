using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public interface IShellHost
{
    IShell Shell { get; }
}

public interface IShell : IRoutable
{
    ReactiveCommand GoHome { get; }
    ValueTask GoHomeAsync(CancellationToken cancel = default);
    IObservableCollection<string[]> ForwardStack { get; }
    ReactiveCommand Forward { get; }
    ValueTask ForwardAsync(CancellationToken cancel = default);
    IObservableCollection<string[]> BackwardStack { get; }
    ReactiveCommand Backward { get; }
    ValueTask BackwardAsync(CancellationToken cancel = default);
    ReadOnlyReactiveProperty<IRoutable> SelectedControl { get; }
    ReadOnlyReactiveProperty<string[]> SelectedControlPath { get; }
    IReadOnlyObservableList<IPage> Pages { get; }
}
