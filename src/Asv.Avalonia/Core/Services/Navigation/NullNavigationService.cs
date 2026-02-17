using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public sealed class NullNavigationService : INavigationService
{
    public static INavigationService Instance { get; } = new NullNavigationService();

    private NullNavigationService() { }

    public IShell Shell => DesignTimeShellViewModel.Instance;

    public IObservableCollection<NavigationPath> BackwardStack { get; } =
        new ObservableStack<NavigationPath>();

    public ValueTask BackwardAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ReactiveCommand Backward { get; } = new();
    public IObservableCollection<NavigationPath> ForwardStack { get; } =
        new ObservableList<NavigationPath>();

    public ValueTask ForwardAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ReactiveCommand Forward { get; } = new();
    public ReadOnlyReactiveProperty<IRoutable?> SelectedControl { get; } =
        new ReactiveProperty<IRoutable?>();
    public ReadOnlyReactiveProperty<NavigationPath> SelectedControlPath { get; } =
        new ReactiveProperty<NavigationPath>();

    public ValueTask<IRoutable> GoTo(NavigationPath path)
    {
        return ValueTask.FromResult<IRoutable>(Shell);
    }

    public ValueTask GoHomeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public void ForceFocus(IRoutable? routable)
    {
        return;
    }

    public ReactiveCommand GoHome { get; } = new();
}
