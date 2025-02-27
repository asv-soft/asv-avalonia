using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public sealed class NullNavigationService : INavigationService
{
    public static INavigationService Instance { get; } = new NullNavigationService();

    private NullNavigationService() { }

    public IShell Shell => DesignTimeShellViewModel.Instance;

    public IObservableCollection<string[]> BackwardStack { get; } = new ObservableStack<string[]>();

    public ValueTask BackwardAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ReactiveCommand Backward { get; } = new();
    public IObservableCollection<string[]> ForwardStack { get; } = new ObservableList<string[]>();

    public ValueTask ForwardAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ReactiveCommand Forward { get; } = new();
    public ReadOnlyReactiveProperty<IRoutable?> SelectedControl { get; } =
        new ReactiveProperty<IRoutable?>();
    public ReadOnlyReactiveProperty<string[]> SelectedControlPath { get; } =
        new ReactiveProperty<string[]>();

    public ValueTask<IRoutable> GoTo(string[] path)
    {
        return ValueTask.FromResult<IRoutable>(Shell);
    }

    public IExportInfo Source => SystemModule.Instance;
}
