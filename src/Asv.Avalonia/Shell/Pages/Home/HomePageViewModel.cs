using System.Composition;
using Asv.Cfg;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia;

[ExportPage(PageId)]
public class HomePageViewModel : PageViewModel<IHomePage>, IHomePage
{
    public const string PageId = "home";

    public HomePageViewModel()
        : this(
            NullCommandService.Instance,
            NullAppInfo.Instance,
            NullContainerHost.Instance,
            NullLayoutService.Instance,
            DesignTime.LoggerFactory
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        var items = Enum.GetValues<MaterialIconKind>();
        for (int i = 0; i < 5; i++)
        {
            Tools.Add(
                new ActionViewModel($"cmd{i}", NullLayoutService.Instance, DesignTime.LoggerFactory)
                {
                    Icon = (MaterialIconKind)Random.Shared.Next(1, items.Length),
                    Header = $"Tool {i}",
                    Description = $"Open tool page {i} with description",
                    Order = 0,
                    Command = null,
                    CommandParameter = null,
                }
            );
        }

        for (int i = 0; i < 5; i++)
        {
            var d = new HomePageItem(
                $"dev{i}",
                NullLayoutService.Instance,
                DesignTime.LoggerFactory
            )
            {
                Icon = MaterialIconKind.Drone,
                Header = $"Device {i}",
                Description = $"Device description {i}",
                Order = 0,
            };
            d.Info.Add(
                new HeadlinedViewModel(
                    "prop1",
                    NullLayoutService.Instance,
                    DesignTime.LoggerFactory
                )
                {
                    Icon = MaterialIconKind.IdCard,
                    Header = "StaticId",
                    Description = "Mavlink(1.1)",
                }
            );
            d.Info.Add(
                new HeadlinedViewModel(
                    "prop2",
                    NullLayoutService.Instance,
                    DesignTime.LoggerFactory
                )
                {
                    Icon = MaterialIconKind.MergeType,
                    Header = "Type",
                    Description = "Copter",
                }
            );
            d.Info.Add(
                new HeadlinedViewModel(
                    "prop3",
                    NullLayoutService.Instance,
                    DesignTime.LoggerFactory
                )
                {
                    Icon = MaterialIconKind.SerialPort,
                    Header = "Port",
                    Description = "serial 1",
                }
            );

            for (int j = 0; j < 5; j++)
            {
                d.Actions.Add(
                    new ActionViewModel(
                        $"cmd{i}",
                        NullLayoutService.Instance,
                        DesignTime.LoggerFactory
                    )
                    {
                        Icon = (MaterialIconKind)Random.Shared.Next(1, items.Length),
                        Header = $"Device tool {i}",
                        Description = $"Open tool page {i} with description",
                        Order = 0,
                        Command = null,
                        CommandParameter = null,
                    }
                );
            }

            Items.Add(d);
        }
    }

    [ImportingConstructor]
    public HomePageViewModel(
        ICommandService cmd,
        IAppInfo appInfo,
        IContainerHost container,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory
    )
        : base(PageId, cmd, layoutService, loggerFactory)
    {
        AppInfo = appInfo;
        Icon = MaterialIconKind.Home;
        Title = RS.HomePageViewModel_Title;

        Tools = [];
        Tools.SetRoutableParent(this).DisposeItWith(Disposable);
        Tools.DisposeRemovedItems().DisposeItWith(Disposable);
        ToolsView = Tools.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);

        Items = [];

        ItemsList = Items
            .CreateView(x => new HomePageItemDecorator(x, container, layoutService, loggerFactory))
            .DisposeItWith(Disposable);

        ItemsList.DisposeMany().DisposeItWith(Disposable);
        ItemsList.SetRoutableParent(this).DisposeItWith(Disposable);

        ItemsView = ItemsList.ToNotifyCollectionChanged().DisposeItWith(Disposable);
    }

    public IAppInfo AppInfo
    {
        get;
        set => SetField(ref field, value);
    }

    public NotifyCollectionChangedSynchronizedViewList<HomePageItemDecorator> ItemsView { get; }

    public ISynchronizedView<IHomePageItem, HomePageItemDecorator> ItemsList { get; }

    public ObservableList<IHomePageItem> Items { get; }

    public NotifyCollectionChangedSynchronizedViewList<IActionViewModel> ToolsView { get; }

    public ObservableList<IActionViewModel> Tools { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        foreach (var model in Tools)
        {
            yield return model;
        }

        foreach (var model in Items)
        {
            yield return model;
        }
    }

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }

    public override IExportInfo Source => SystemModule.Instance;
}
