using System.Windows.Input;

namespace Asv.Avalonia;

/// <summary>
/// Represents a view model that supports an actionable command, inheriting header, icon, and description properties.
/// </summary>
public interface IActionViewModel : IHeadlinedViewModel
{
    /// <summary>
    /// Gets or sets the action that is executed when the view model is activated.
    /// </summary>
    ICommand? Command { get; set; }

    /// <summary>
    /// Gets the parameter passed to the command when executed.
    /// </summary>
    object? CommandParameter { get; }
}
