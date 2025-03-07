using System.Collections.Immutable;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public interface INavigationService : IExportable
{
    IObservableCollection<NavigationPath> BackwardStack { get; }
    ValueTask BackwardAsync();
    ReactiveCommand Backward { get; }
    IObservableCollection<NavigationPath> ForwardStack { get; }
    ValueTask ForwardAsync();
    ReactiveCommand Forward { get; }
    ReadOnlyReactiveProperty<IRoutable?> SelectedControl { get; }
    ReadOnlyReactiveProperty<NavigationPath> SelectedControlPath { get; }
    ValueTask<IRoutable> GoTo(NavigationPath path);
    ValueTask GoHomeAsync();
    ReactiveCommand GoHome { get; }
}
