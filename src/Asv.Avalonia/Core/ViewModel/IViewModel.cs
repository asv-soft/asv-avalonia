using System.ComponentModel;
using Asv.Common;

namespace Asv.Avalonia;

/// <summary>
/// Defines a base contract for all view models in the application.
/// This interface provides a unique identifier, supports property change notifications,
/// and ensures proper disposal of resources.
/// </summary>
public interface IViewModel : IDisposable, INotifyPropertyChanged, ISupportId<NavigationId>
{
    void InitArgs(string? args);

    /// <summary>
    /// Gets a value indicating whether the view model has been disposed.
    /// </summary>
    bool IsDisposed { get; }
}
