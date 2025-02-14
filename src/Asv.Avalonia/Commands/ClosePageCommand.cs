using System.Composition;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
public class ClosePageCommand : ContextCommand<IPage>
{
    #region Static

    public const string Id = $"{BaseId}.page.close";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Close page",
        Description = RS.OpenDebugCommand_CommandInfo_Description,
        Icon = MaterialIconKind.CloseBold,
        DefaultHotKey = KeyGesture.Parse("Ctrl+Q"),
        Source = SystemModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<IPersistable?> InternalExecute(
        IPage context,
        IPersistable newValue,
        CancellationToken cancel
    )
    {
        await context.TryCloseAsync();
        return null;
    }
}
