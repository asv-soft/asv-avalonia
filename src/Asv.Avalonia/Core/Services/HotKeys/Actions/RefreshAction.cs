using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

public class RefreshAction : HotKeyAction<ISupportRefresh>
{
    public const string Id = "refresh";
    public override string ActionId => Id;
    public override string Name => RS.RefreshCommand_CommandInfo_Name;
    public override string Description => RS.RefreshCommand_CommandInfo_Description;
    public override MaterialIconKind Icon => MaterialIconKind.Refresh;
    public override KeyGesture DefaultHotKey => new(Key.F5);

    protected override async ValueTask Execute(ISupportRefresh target, CancellationToken cancel)
    {
        await target.Refresh();
    }
}
