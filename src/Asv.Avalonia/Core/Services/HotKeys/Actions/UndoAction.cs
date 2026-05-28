using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

public class UndoAction : HotKeyAction<IPage>
{
    public const string Id = "undo";
    public const MaterialIconKind IconKind = MaterialIconKind.Undo;

    public override string ActionId => Id;
    public override string Name => RS.UndoCommand_CommandInfo_Name;
    public override string Description => RS.UndoCommand_CommandInfo_Description;
    public override MaterialIconKind Icon => IconKind;
    public override KeyGesture DefaultHotKey => new(Key.Z, KeyModifiers.Control);

    protected override async ValueTask InternalExecute(IPage target, CancellationToken cancel)
    {
        await target.UndoHistory.UndoAsync(cancel);
    }
}
