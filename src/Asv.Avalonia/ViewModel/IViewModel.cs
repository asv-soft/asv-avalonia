using System.ComponentModel;

namespace Asv.Avalonia;

public interface IViewModel : IDisposable, INotifyPropertyChanged
{
    string Id { get; }
    bool IsDisposed { get; }
}
