using Avalonia.Controls;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public interface IShellHost
{
    IShell Shell { get; }
    Observable<IShell> OnShellLoaded { get; }
    TopLevel TopLevel { get; }
}

public class NullShellHost : IShellHost
{
    public static IShellHost Instance { get; } = new NullShellHost();

    private NullShellHost() { }

    public IShell Shell => DesignTimeShellViewModel.Instance;
    public Observable<IShell> OnShellLoaded { get; } = new Subject<IShell>();
    public TopLevel TopLevel { get; } = null!; // TODO: Create a DesignTime version
}

public enum ShellErrorState
{
    Normal,
    Warning,
    Error,
}

public interface IShell : IRoutable
{
    string Title { get; set; }
    ShellErrorState ErrorState { get; set; }
    ObservableList<IMenuItem> MainMenu { get; }
    IReadOnlyObservableList<IPage> Pages { get; }
    BindableReactiveProperty<IPage?> SelectedPage { get; }
    ObservableList<IStatusItem> StatusItems { get; }

    ObservableList<IMenuItem> LeftMenu { get; }
    ObservableList<IMenuItem> RightMenu { get; }
}
