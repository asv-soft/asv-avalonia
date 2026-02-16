using Asv.Common;
using Asv.IO;
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

public class HomePageItem : ExtendableHeadlinedViewModel<IHomePageItem>, IHomePageItem
{
    public HomePageItem(NavigationId id, ILoggerFactory loggerFactory, IExtensionService ext)
        : base(id, loggerFactory, ext)
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

    public override IEnumerable<IRoutable> GetChildren()
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

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }
}
