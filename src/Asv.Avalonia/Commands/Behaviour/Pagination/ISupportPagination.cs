using R3;

namespace Asv.Avalonia;

public interface ISupportPagination : IRoutable
{
    BindableReactiveProperty<int> Skip { get; }
    BindableReactiveProperty<int> Take { get; }
}

public interface ISupportPaginationCommands
{
    ICommandInfo NextPageCommand { get; }
    ValueTask NextPage(IRoutable context);
    ICommandInfo PreviousPageCommand { get; }
    ValueTask PreviousPage(IRoutable context);
    ICommandInfo GoToPageCommand { get; }
    ValueTask GoToPage(IRoutable context, int skip, int take);
}
