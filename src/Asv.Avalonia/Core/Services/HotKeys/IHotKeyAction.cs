namespace Asv.Avalonia;

public interface IHotKeyAction : IHotKeyInfo
{
    bool CanExecute(IViewModel context);
    ValueTask Execute(IViewModel context, CancellationToken cancel = default);
}
