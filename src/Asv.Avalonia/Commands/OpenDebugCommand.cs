using System.Composition;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
[method: ImportingConstructor]
public class OpenDebugWindowCommand(ExportFactory<IDebugWindow> factory) : NoContextCommand
{
    #region Static

    public const string Id = $"{BaseId}.open.debug";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.OpenDebugCommand_CommandInfo_Name,
        Description = RS.OpenDebugCommand_CommandInfo_Description,
        Icon = MaterialIconKind.WindowOpenVariant,
        DefaultHotKey = KeyGesture.Parse("Ctrl+D"),
        Source = SystemModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    protected override ValueTask<ICommandArg?> InternalExecute(
        ICommandArg newValue,
        CancellationToken cancel
    )
    {
        var wnd = new DebugWindow { DataContext = factory.CreateExport().Value, Topmost = true };
        wnd.Show();
        return ValueTask.FromResult<ICommandArg?>(null);
    }
}
