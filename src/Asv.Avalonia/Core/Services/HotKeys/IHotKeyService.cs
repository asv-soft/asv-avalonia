using Avalonia.Input;
using R3;

namespace Asv.Avalonia;

/// <summary>
/// Dispatches global keyboard shortcuts and stores user overrides.
/// </summary>
public interface IHotKeyService
{
    /// <summary>
    /// Gets an observable that emits every recognized gesture before it is dispatched to an action.
    /// </summary>
    Observable<KeyGesture> OnHotKey { get; }

    /// <summary>
    /// Gets or sets a value indicating whether global hot key handling is enabled.
    /// </summary>
    bool IsHotKeyEnabled { get; set; }

    /// <summary>
    /// Gets or sets the gesture bound to an action identifier.
    /// </summary>
    /// <param name="hotKeyId">The action identifier.</param>
    /// <returns>The effective gesture assigned to the action.</returns>
    /// <remarks>Setting the value to <see langword="null"/> restores the default gesture.
    /// A gesture with Key.None explicitly disables the action.</remarks>
    KeyGesture? this[string hotKeyId] { get; set; }

    /// <summary>
    /// Applies user assignments. Null restores the default; Key.None disables an action.
    /// Implementations may override this method to persist the batch in one operation.
    /// </summary>
    void ApplyHotKeys(IReadOnlyDictionary<string, KeyGesture?> assignments)
    {
        foreach (var (id, gesture) in assignments)
        {
            this[id] = gesture;
        }
    }

    /// <summary>
    /// Gets all registered actions.
    /// </summary>
    IEnumerable<IHotKeyInfo> Actions { get; }

    /// <summary>
    /// Gets an observable that emits when an action's gesture changes.
    /// </summary>
    Observable<(IHotKeyInfo Action, KeyGesture Gesture)> OnHotKeyGestureChanged { get; }

    /// <summary>
    /// Reports whether an action can execute for the current context.
    /// </summary>
    /// <param name="actionId">The action identifier.</param>
    /// <returns>An observable sequence containing the current execution state.</returns>
    Observable<bool> ObserveCanExecute(string actionId);
}
