using Asv.Avalonia.Save;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

public class SaveAction : HotKeyAction<ISupportSave>
{
    public const string Id = "save";
    public override string ActionId => Id;
    public override string Name => RS.SaveCommand_CommandInfo_Name;
    public override string Description => RS.SaveCommand_CommandInfo_Description;
    public override MaterialIconKind Icon => MaterialIconKind.FloppyDisc;
    public override KeyGesture DefaultHotKey => new(Key.S, KeyModifiers.Control);
    protected override ValueTask<bool> Execute(ISupportSave target, CancellationToken cancel)
    {
        target.Save();
        return new ValueTask<bool>(true);
    }
}