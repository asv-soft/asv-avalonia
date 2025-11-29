using System.Composition;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
public class PreviousPageCommand : ContextCommand<ISupportPagination>
{
    public const string Id = $"{BaseId}.page.back";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.PreviousPageCommand_CommandInfo_Name,
        Description = RS.PreviousPageCommand_CommandInfo_Description,
        Icon = MaterialIconKind.ArrowLeftBold,
        DefaultHotKey = "Ctrl+Left",
        Source = SystemModule.Instance,
    };
    public override ICommandInfo Info => StaticInfo;

    public static ValueTask ExecuteAtContext(IRoutable context) =>
        context.ExecuteCommand(Id, CommandArg.Empty);

    protected override async ValueTask<CommandArg?> InternalExecute(
        ISupportPagination context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        // we just re-execute the pagination command with incremented skip value
        // it will save the old pagination values and refresh the page
        var temp = context.Skip.Value - context.Take.Value;
        await Commands.SetPagination(context, temp < 0 ? 0 : temp, context.Take.Value);
        return null;
    }
}
