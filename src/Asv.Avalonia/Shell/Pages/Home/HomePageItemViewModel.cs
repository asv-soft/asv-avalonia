using Asv.Common;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia;

public class HomePageItemViewModel : RoutableViewModel
{
    public HomePageItemViewModel(
        IHomePageItem homePageItem,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory
    )
        : base(homePageItem.Id, layoutService, loggerFactory)
    {
        HomePageItem = homePageItem;
        ActionsView = homePageItem
            .Actions.ToNotifyCollectionChangedSlim()
            .DisposeItWith(Disposable);
        PropertiesView = homePageItem
            .Info.ToNotifyCollectionChangedSlim()
            .DisposeItWith(Disposable);
        HomePageItem.Parent = this;
    }

    public IHomePageItem HomePageItem { get; }
    public NotifyCollectionChangedSynchronizedViewList<IActionViewModel> ActionsView { get; }
    public NotifyCollectionChangedSynchronizedViewList<IHeadlinedViewModel> PropertiesView { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        foreach (var model in PropertiesView)
        {
            yield return model;
        }

        foreach (var action in ActionsView)
        {
            yield return action;
        }
    }
}
