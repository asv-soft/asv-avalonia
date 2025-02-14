using System.Collections.Immutable;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public interface INavigationService : IExportable
{
    IObservableCollection<string[]> BackwardStack { get; }
    ValueTask BackwardAsync();
    ReactiveCommand Backward { get; }
    IObservableCollection<string[]> ForwardStack { get; }
    ValueTask ForwardAsync();
    ReactiveCommand Forward { get; }
    ReadOnlyReactiveProperty<IRoutable?> SelectedControl { get; }
    ReadOnlyReactiveProperty<string[]> SelectedControlPath { get; }
    ValueTask<IRoutable> GoTo(string[] path);
}
