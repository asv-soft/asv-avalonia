using Asv.Avalonia.Routable;
using Asv.Common;
using ObservableCollections;

namespace Asv.Avalonia;

public class HomePageItemDecorator : ExtendableViewModel<IHomePageItem>
{
    public HomePageItemDecorator(IHomePageItem homePageItem, IContainerHost container)
        : base($"decorator_{homePageItem.Id}")
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
        container.SatisfyImports(this);
    }

    public IHomePageItem HomePageItem { get; }
    public NotifyCollectionChangedSynchronizedViewList<IActionViewModel> ActionsView { get; }
    public NotifyCollectionChangedSynchronizedViewList<IHeadlinedViewModel> PropertiesView { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        // this is a decorator, it should not be routable
        throw new NotImplementedException();
    }

    public override ValueTask<IRoutable> Navigate(Routable.NavigationId id)
    {
        // this is a decorator, it should not be routable
        throw new NotImplementedException();
    }

    protected override IHomePageItem GetContext()
    {
        return HomePageItem;
    }

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }
}
