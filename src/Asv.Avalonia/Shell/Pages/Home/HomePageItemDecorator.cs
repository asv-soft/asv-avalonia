using Asv.Common;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia;

public class HomePageItemDecorator : RoutableViewModel
{
    public HomePageItemDecorator(IHomePageItem homePageItem, ILoggerFactory loggerFactory)
        : base($"decorator_{homePageItem.Id}", loggerFactory)
    {
        homePageItem.Parent = this;
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

    public override IEnumerable<IRoutable> GetChildren()
    {
        // this is a decorator, it should not be routable
        throw new NotImplementedException();
    }

    public override ValueTask<IRoutable> Navigate(NavigationId id)
    {
        // this is a decorator, it should not be routable
        throw new NotImplementedException();
    }
}
