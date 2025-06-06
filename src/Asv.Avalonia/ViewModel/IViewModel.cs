using System.ComponentModel;
using System.Diagnostics;

namespace Asv.Avalonia;

/// <summary>
/// Defines a base contract for all view models in the application.
/// This interface provides a unique identifier, supports property change notifications,
/// and ensures proper disposal of resources.
/// </summary>
public interface IViewModel : IDisposable, INotifyPropertyChanged
{
    /// <summary>
    /// Gets the unique identifier of the view model.
    /// This can be used to differentiate instances within the application.
    /// </summary>
    Routable.NavigationId Id { get; }

    void InitArgs(string? args);

    /// <summary>
    /// Gets a value indicating whether the view model has been disposed.
    /// </summary>
    bool IsDisposed { get; }
}
