using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

public class RedoAction : HotKeyAction<IPage>
{
    public const string Id = "redo";
    public override string ActionId => Id;
    public override string Name => RS.RedoCommand_CommandInfo_Name;
    public override string Description => RS.RedoCommand_CommandInfo_Description;
    public override MaterialIconKind Icon => MaterialIconKind.Undo;
    public override KeyGesture DefaultHotKey => new(Key.Z, KeyModifiers.Control);
    protected override async ValueTask<bool> Execute(IPage target, CancellationToken cancel)
    {
        await target.UndoHistory.UndoAsync(cancel);
        return true;
    }
}