using Asv.Common;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia;

public interface IHomePage : IPage
{
    IAppInfo AppInfo { get; set; }
    ObservableList<IActionViewModel> Tools { get; }
    ObservableList<IHomePageItem> Items { get; }
}

public interface IHomePageItem : IHeadlinedViewModel
{
    ObservableList<IActionViewModel> Actions { get; }
    ObservableList<IHeadlinedViewModel> Info { get; }
}

public class HomePageItem : HeadlinedViewModel, IHomePageItem
{
    public HomePageItem(NavigationId id, ILoggerFactory loggerFactory)
        : base(id, loggerFactory)
    {
        Disposable.AddAction(() => Actions.Clear());
        Disposable.AddAction(() => Info.Clear());

        Actions.SetRoutableParent(this).DisposeItWith(Disposable);
        Actions.DisposeRemovedItems().DisposeItWith(Disposable);

        Info.SetRoutableParent(this).DisposeItWith(Disposable);
        Info.DisposeRemovedItems().DisposeItWith(Disposable);
    }

    public ObservableList<IActionViewModel> Actions { get; } = [];
    public ObservableList<IHeadlinedViewModel> Info { get; } = [];

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        foreach (var model in Actions)
        {
            yield return model;
        }

        foreach (var action in Info)
        {
            yield return action;
        }
    }
}
