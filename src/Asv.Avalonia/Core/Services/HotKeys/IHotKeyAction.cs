namespace Asv.Avalonia;

public interface IHotKeyAction : IHotKeyInfo
{
    ValueTask TryExecute(IViewModel context, CancellationToken cancel = default);
}
