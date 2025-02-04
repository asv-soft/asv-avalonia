using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Asv.Avalonia;

public abstract class ViewModelBase(string id) : IViewModel
{
    private volatile int _isDisposed;

    public string Id { get; } = id;

    #region Dispose

    public bool IsDisposed => _isDisposed != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ThrowIfDisposed()
    {
        if (_isDisposed == 0)
        {
            return;
        }

        throw new ObjectDisposedException(GetType().FullName);
    }

    public void Dispose()
    {
        // Make sure we're the first call to Dispose
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1)
        {
            return;
        }

        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected abstract void Dispose(bool disposing);

    #endregion

    #region PropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}
