using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

public class RedoAction : HotKeyAction<IPage>
{
    public const string Id = "redo";
    public const MaterialIconKind IconKind = MaterialIconKind.Redo;

    public override string ActionId => Id;
    public override string Name => RS.RedoCommand_CommandInfo_Name;
    public override string Description => RS.RedoCommand_CommandInfo_Description;
    public override MaterialIconKind Icon => IconKind;
    public override KeyGesture DefaultHotKey => new(Key.Y, KeyModifiers.Control);

    protected override async ValueTask InternalExecute(IPage target, CancellationToken cancel)
    {
        await target.UndoHistory.RedoAsync(cancel);
    }
}
