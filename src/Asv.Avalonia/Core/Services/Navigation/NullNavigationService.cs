using Asv.Modeling;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public sealed class NullNavigationService : INavigationService
{
    public static INavigationService Instance { get; } = new NullNavigationService();

    private NullNavigationService() { }

    public IShell Shell => DesignTimeShellViewModel.Instance;

    public IObservableCollection<NavPath> BackwardStack { get; } =
        new ObservableStack<NavPath>();

    public ValueTask BackwardAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ReactiveCommand Backward { get; } = new();
    public IObservableCollection<NavPath> ForwardStack { get; } =
        new ObservableList<NavPath>();

    public ValueTask ForwardAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ReactiveCommand Forward { get; } = new();
    public ReadOnlyReactiveProperty<IViewModel?> SelectedControl { get; } =
        new ReactiveProperty<IViewModel?>();
    public ReadOnlyReactiveProperty<NavPath> SelectedControlPath { get; } =
        new ReactiveProperty<NavPath>();

    public ValueTask<IViewModel> GoTo(NavPath path)
    {
        return ValueTask.FromResult<IViewModel>(Shell);
    }

    public ValueTask GoHomeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public void ForceFocus(IViewModel? routable)
    {
        return;
    }

    public ReactiveCommand GoHome { get; } = new();
}
