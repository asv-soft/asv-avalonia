using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

/// <summary>
/// Provides a read-only description of a hot key action.
/// </summary>
public interface IHotKeyInfo
{
    /// <summary>
    /// Gets the unique action identifier.
    /// </summary>
    string ActionId { get; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the human-readable description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the icon associated with the action.
    /// </summary>
    MaterialIconKind Icon { get; }

    /// <summary>
    /// Gets the gesture used when no user override is configured.
    /// </summary>
    KeyGesture DefaultHotKey { get; }
}
