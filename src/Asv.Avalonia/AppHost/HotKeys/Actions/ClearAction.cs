using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

public class ClearAction : HotKeyAction<ISupportClear>
{
    public const string Id = "clear";
    public override string ActionId => Id;
    public override string Name => RS.ClearCommand_CommandInfo_Name;
    public override string Description => RS.ClearCommand_CommandInfo_Description;
    public override MaterialIconKind Icon => MaterialIconKind.Clear;
    public override KeyGesture DefaultHotKey => new(Key.Escape, KeyModifiers.Control);
    protected override ValueTask<bool> Execute(ISupportClear target, CancellationToken cancel)
    {
        target.Clear();
        return new ValueTask<bool>(true);
    }
}