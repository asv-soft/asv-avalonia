using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

public class CancelAction : HotKeyAction<ISupportCancel>
{
    public const string Id = "cancel";
    public override string ActionId => Id;
    public override string Name => RS.CancelCommand_CommandInfo_Name;
    public override string Description => RS.CancelCommand_CommandInfo_Description;
    public override MaterialIconKind Icon => MaterialIconKind.Cancel; 
    public override KeyGesture DefaultHotKey { get; } = new(Key.Escape);
    protected override ValueTask<bool> Execute(ISupportCancel target, CancellationToken cancel)
    {
        target.Cancel();
        return new ValueTask<bool>(true);
    }
}