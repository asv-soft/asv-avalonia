using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

public class UndoAction : HotKeyAction<IPage>
{
    public override string ActionId => "undo";
    public override string Name => "Undo";
    public override string Description => "Undo last action on page";
    public override MaterialIconKind Icon => MaterialIconKind.Undo;
    public override KeyGesture DefaultHotKey => new(Key.Z, KeyModifiers.Control);
    protected override async ValueTask<bool> Execute(IPage target, CancellationToken cancel)
    {
        await target.UndoHistory.UndoAsync(cancel);
        return true;
    }
}