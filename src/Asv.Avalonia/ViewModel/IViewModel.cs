namespace Asv.Avalonia;

public interface IViewModel : IDisposable
{
    string Id { get; }
    bool IsDisposed { get; }
}
