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
        Name = RS.ClosePageCommand_CommandInfo_Name,
        Description = RS.ClosePageCommand_CommandInfo_Description,
        Icon = MaterialIconKind.CloseBold,
        DefaultHotKey = KeyGesture.Parse("Ctrl+Q"),
        Source = SystemModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<ICommandArg?> InternalExecute(
        IPage context,
        ICommandArg newValue,
        CancellationToken cancel
    )
    {
        await context.TryCloseAsync();
        return null;
    }
}
