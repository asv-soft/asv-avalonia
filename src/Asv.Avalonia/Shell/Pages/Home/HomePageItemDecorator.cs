using Asv.Common;
using Asv.Modeling;
using ObservableCollections;

namespace Asv.Avalonia;

public class HomePageItemDecorator : ViewModel
{
    public HomePageItemDecorator(IHomePageItem homePageItem)
        : base($"decorator")
    {
        homePageItem.SetParent(this);
        HomePageItem = homePageItem;
        HomePageItem.DisposeItWith(Disposable);
        ActionsView = homePageItem
            .Actions.ToNotifyCollectionChangedSlim()
            .DisposeItWith(Disposable);
        PropertiesView = homePageItem
            .Info.ToNotifyCollectionChangedSlim()
            .DisposeItWith(Disposable);
    }

    public IHomePageItem HomePageItem { get; }
    public NotifyCollectionChangedSynchronizedViewList<IActionViewModel> ActionsView { get; }
    public NotifyCollectionChangedSynchronizedViewList<IHeadlinedViewModel> PropertiesView { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        throw new NotImplementedException("this is a decorator, it should not be routable");
    }

    public override ValueTask<IViewModel> Navigate(NavId id, CancellationToken cancel = default)
    {
        throw new NotImplementedException("this is a decorator, it should not be routable");
    }
}
