namespace Asv.Avalonia;

public interface IHotKeyAction : IHotKeyInfo
{
    ValueTask<bool> TryExecute(IViewModel context, CancellationToken cancel = default);
}