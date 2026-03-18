using Material.Icons;

namespace Asv.Avalonia;

public class PaginationCommand : ContextCommand<ISupportPagination, ListArg>
{
    public const string Id = $"{BaseId}.page.pagination";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.PaginationCommand_CommandInfo_Name,
        Description = RS.PaginationCommand_CommandInfo_Description,
        Icon = MaterialIconKind.ViewList,
        DefaultHotKey = null,
    };

    public override ICommandInfo Info => StaticInfo;

    public static ValueTask ExecuteAtContext(IRoutable context, int skip, int take) =>
        context.ExecuteCommand(Id, new ListArg(2) { new IntArg(skip), new IntArg(take) });

    public override ValueTask<ListArg?> InternalExecute(
        ISupportPagination context,
        ListArg arg,
        CancellationToken cancel
    )
    {
        var old = new ListArg(2) { new IntArg(context.Skip.Value), new IntArg(context.Take.Value) };
        var skip = ((IntArg)arg[0]).Value;
        var take = ((IntArg)arg[1]).Value;
        context.Skip.Value = (int)skip;
        context.Take.Value = (int)take;
        return ValueTask.FromResult<ListArg?>(old);
    }
}
