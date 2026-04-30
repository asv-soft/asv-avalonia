using System.Windows.Input;
using Avalonia.Controls;
using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public enum ShellErrorState
{
    Normal,
    Warning,
    Error,
}

public interface IShell : IHasHeader
{
    Observable<Unit> OnClose { get; }
    ObservableList<IMenuItem> MainMenu { get; }
    ObservableList<IMenuItem> LeftMenu { get; }
    ObservableList<IMenuItem> RightMenu { get; }
    IReadOnlyObservableList<IPage> Pages { get; }
    BindableReactiveProperty<IPage?> SelectedPage { get; }
    ObservableList<IStatusItem> StatusItems { get; }
    ShellErrorState ErrorState { get; set; }
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
