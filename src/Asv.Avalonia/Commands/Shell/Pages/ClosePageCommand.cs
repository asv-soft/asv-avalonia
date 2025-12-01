using System.Composition;
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
        DefaultHotKey = "Ctrl+Q",
        Source = SystemModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<CommandArg?> InternalExecute(
        IPage context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        var isForce = false;
        if (newValue is BoolArg b)
        {
            isForce = b.Value;
        }

        await context.TryCloseAsync(isForce);
        return null;
    }
}
