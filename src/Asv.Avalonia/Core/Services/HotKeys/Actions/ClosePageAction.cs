using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

public class ClosePageAction : HotKeyAction<IPage>
{
    public const string Id = "close_page";
    public override string ActionId => Id;
    public override string Name => RS.ClosePageCommand_CommandInfo_Name;
    public override string Description => RS.ClosePageCommand_CommandInfo_Description;
    public override MaterialIconKind Icon => MaterialIconKind.CloseBold;
    public override KeyGesture DefaultHotKey => new(Key.Q, KeyModifiers.Control);

    protected override ValueTask InternalExecute(IPage target, CancellationToken cancel)
    {
        return target.TryCloseAsync(false, cancel);
    }
}
