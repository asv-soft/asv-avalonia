using R3;

namespace Asv.Avalonia;

public interface ISupportPagination : IViewModel
{
    BindableReactiveProperty<int> Skip { get; }
    BindableReactiveProperty<int> Take { get; }
}

public interface ISupportPaginationCommands
{
    ValueTask NextPage(IViewModel context);
    ValueTask PreviousPage(IViewModel context);
    ValueTask GoToPage(IViewModel context, int skip, int take);
}
