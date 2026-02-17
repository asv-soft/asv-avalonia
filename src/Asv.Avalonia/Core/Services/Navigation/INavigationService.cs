using ObservableCollections;
using R3;

namespace Asv.Avalonia;

/// <summary>
/// Represents a navigation service for handling application navigation, navigation history, and focus management.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Gets the observable collection representing the backward navigation history.
    /// </summary>
    IObservableCollection<NavigationPath> BackwardStack { get; }

    /// <summary>
    /// Navigates to the previous item in the backward navigation stack.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask BackwardAsync();

    /// <summary>
    /// Gets the <see cref="ReactiveCommand"/> that triggers backward navigation.
    /// </summary>
    ReactiveCommand Backward { get; }

    /// <summary>
    /// Gets the observable collection representing the forward navigation history.
    /// </summary>
    IObservableCollection<NavigationPath> ForwardStack { get; }

    /// <summary>
    /// Navigates to the next item in the forward navigation stack.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing an asynchronous operation.</returns>
    ValueTask ForwardAsync();

    /// <summary>
    /// Gets the <see cref="ReactiveCommand"/> that triggers forward navigation.
    /// </summary>
    ReactiveCommand Forward { get; }

    /// <summary>
    /// Gets the currently selected (focused) <see cref="IRoutable"/>.
    /// </summary>
    ReadOnlyReactiveProperty<IRoutable?> SelectedControl { get; }

    /// <summary>
    /// Gets the <see cref="NavigationPath"/> of the currently selected control.
    /// </summary>
    ReadOnlyReactiveProperty<NavigationPath> SelectedControlPath { get; }

    /// <summary>
    /// Navigates to the specified navigation path.
    /// </summary>
    /// <param name="path">Path to navigate to.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> that represents an asynchronous operation, returning the navigated <see cref="IRoutable"/>.
    /// </returns>
    ValueTask<IRoutable> GoTo(NavigationPath path);

    /// <summary>
    /// Navigates to the home page.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing an asynchronous operation.</returns>
    ValueTask GoHomeAsync();

    /// <summary>
    /// Forces focus change to the specified routable control.
    /// </summary>
    /// <param name="routable">Routable control to be set as currently focused.</param>
    void ForceFocus(IRoutable? routable);

    /// <summary>
    /// Gets the <see cref="ReactiveCommand"/> that triggers navigation to the home page.
    /// </summary>
    ReactiveCommand GoHome { get; }
}
