using System.Windows.Input;
using Avalonia.Controls;
using Material.Icons;
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

    void ShowMessage(ShellMessage message);
}

public readonly struct ShellMessage(
    string title,
    string message,
    ShellErrorState severity,
    string? description = null,
    MaterialIconKind? icon = null,
    ICommand? command = null,
    object? commandParam = null,
    string? commandTitle = null,
    TimeSpan? duration = null
)
{
    public string Title => title;
    public string Message => message;
    public ShellErrorState Severity => severity;
    public MaterialIconKind? Icon => icon;
    public ICommand? Command => command;
    public object? CommandParam => commandParam;
    public string? CommandTitle => commandTitle;
    public TimeSpan? Duration => duration;
    public string? Description => description;
}
