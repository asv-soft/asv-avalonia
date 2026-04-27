using System.ComponentModel;
using Asv.Modeling;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

/// <summary>
/// Defines a base contract for all view models in the application.
/// This interface provides a unique identifier, supports property change notifications,
/// and ensures proper disposal of resources.
/// </summary>
public interface IViewModel
    : IDisposable,
        INotifyPropertyChanging,
        INotifyPropertyChanged,
        ISupportUndo<IViewModel>
{
}


