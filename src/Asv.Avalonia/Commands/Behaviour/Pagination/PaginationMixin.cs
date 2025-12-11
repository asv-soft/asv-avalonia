namespace Asv.Avalonia;

public partial class Commands
{
    public static ICommandInfo RefreshPageCommand => PaginationCommand.StaticInfo;

    public static ValueTask SetPagination(IRoutable context, int skip, int take) =>
        context.ExecuteCommand(
            PaginationCommand.Id,
            new ListArg(2) { new IntArg(skip), new IntArg(take) }
        );

    public static ValueTask NextPage(IRoutable context) =>
        context.ExecuteCommand(NextPageCommand.Id);

    public static ValueTask PreviousPage(IRoutable context) =>
        context.ExecuteCommand(PreviousPageCommand.Id);
}
