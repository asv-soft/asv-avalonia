using System.Windows.Input;
using Asv.Modeling;
using Avalonia.Controls;
using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public enum ShellErrorState
{
    Normal = 0,
    Warning = 1,
    Error = 2,
    Success = 3,
}

/// <summary>
/// Defines the application's navigation root, open pages and extension points for shell controls.
/// </summary>
/// <remarks>
/// Modify UI-facing collections and page selection on the UI thread.
/// The built-in implementation is <see cref="ShellViewModel"/>; plugins can contribute controls through shell extensions.
/// </remarks>
public interface IShell : IHasHeader, INavigationRoot<IViewModel>, ISupportLayoutManager<IViewModel>
{
    /// <summary>
    /// Gets notifications that a shell-close request has been approved.
    /// </summary>
    /// <value>A stream that emits after close confirmation and page approval, before the window closes.</value>
    Observable<Unit> OnClose { get; }

    /// <summary>
    /// Gets the items used to build the shell's main application menu.
    /// </summary>
    /// <value>The mutable menu contributions, including nested items linked by <see cref="IMenuItem.ParentId"/>.</value>
    ObservableList<IMenuItem> MainMenu { get; }

    /// <summary>
    /// Gets the menu contributions displayed in the left vertical toolbar.
    /// </summary>
    /// <value>The items rendered as icon buttons with optional child menus, ordered by their menu order.</value>
    ObservableList<IMenuItem> LeftMenu { get; }

    /// <summary>
    /// Gets the menu contributions displayed in the right vertical toolbar.
    /// </summary>
    /// <value>The items rendered as icon buttons with optional child menus, ordered by their menu order.</value>
    /// <remarks>Use <see cref="HeaderItems"/> for custom controls beside Settings in the window header.</remarks>
    ObservableList<IMenuItem> RightMenu { get; }

    /// <summary>
    /// Gets the models rendered beside Settings through the view locator, in insertion order.
    /// </summary>
    /// <value>The mutable collection of models for custom header controls, such as user avatars or account menus.</value>
    /// <remarks>
    /// Register a view for each model type through the view locator.
    /// The shell assigns itself as the models' parent and disposes models removed from the collection,
    /// including through replacement or clearing, and remaining models on shutdown.
    /// Remove an extension's contributions when that extension unloads; do not share models between shells.
    /// Built-in shells support this collection. Custom implementations must override the default getter to support it.
    /// </remarks>
    /// <exception cref="NotSupportedException">The shell does not support header items; the default getter always throws.</exception>
    ObservableList<IViewModel> HeaderItems => throw new NotSupportedException("This shell does not support header items.");

    /// <summary>
    /// Gets the live collection of pages currently open in the shell.
    /// </summary>
    /// <value>A read-only observable view of pages owned by the shell.</value>
    /// <remarks>Use navigation and page-close operations to open or close pages.</remarks>
    IReadOnlyObservableList<IPage> Pages { get; }

    /// <summary>
    /// Gets the reactive property containing the currently selected page.
    /// </summary>
    /// <value>The page selection, whose value can be <see langword="null"/> when no page is selected.</value>
    BindableReactiveProperty<IPage?> SelectedPage { get; }

    /// <summary>
    /// Gets the models displayed in the shell's bottom status bar.
    /// </summary>
    /// <value>The mutable status contributions, ordered by <see cref="IStatusItem.Order"/> in built-in shells.</value>
    /// <remarks>Register a view for each status-model type through the view locator.</remarks>
    ObservableList<IStatusItem> StatusItems { get; }
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
