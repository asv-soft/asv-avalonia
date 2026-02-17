using Material.Icons;

namespace Asv.Avalonia;

public class NextPageCommand : ContextCommand<ISupportPagination>
{
    public const string Id = $"{BaseId}.page.next";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.NextPageCommand_CommandInfo_Name,
        Description = RS.NextPageCommand_CommandInfo_Description,
        Icon = MaterialIconKind.ArrowRightBold,
        DefaultHotKey = "Ctrl+Right",
    };

    public static ValueTask ExecuteAtContext(IRoutable context)
    {
        return context.ExecuteCommand(Id, CommandArg.Empty);
    }

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<CommandArg?> InternalExecute(
        ISupportPagination context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        // we just re-execute the pagination command with incremented skip value
        // it will save the old pagination values and refresh the page
        await Commands.SetPagination(
            context,
            context.Skip.Value + context.Take.Value,
            context.Take.Value
        );
        return null;
    }
}
