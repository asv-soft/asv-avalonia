using R3;

namespace Asv.Avalonia;

public interface ISupportPagination : IViewModel
{
    BindableReactiveProperty<int> Skip { get; }
    BindableReactiveProperty<int> Take { get; }
}

public interface ISupportPaginationCommands
{
    ICommandInfo NextPageCommand { get; }
    ValueTask NextPage(IViewModel context);
    ICommandInfo PreviousPageCommand { get; }
    ValueTask PreviousPage(IViewModel context);
    ICommandInfo GoToPageCommand { get; }
    ValueTask GoToPage(IViewModel context, int skip, int take);
}
