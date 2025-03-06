﻿using System.Composition;
using Asv.Common;
using Material.Icons;
using ObservableCollections;

namespace Asv.Avalonia;

[ExportPage(PageId)]
public class HomePageViewModel : PageViewModel<IHomePage>, IHomePage
{
    public const string PageId = "home";

    public HomePageViewModel()
        : this(NullCommandService.Instance, NullAppInfo.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
        var items = Enum.GetValues<MaterialIconKind>();
        for (int i = 0; i < 5; i++)
        {
            Tools.Add(
                new ActionViewModel($"cmd{i}")
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
            var d = new HomePageItem($"dev{i}")
            {
                Icon = MaterialIconKind.Drone,
                Header = $"Device {i}",
                Description = $"Device description {i}",
                Order = 0,
            };
            d.Info.Add(
                new HeadlinedViewModel("prop1")
                {
                    Icon = MaterialIconKind.IdCard,
                    Header = "Id",
                    Description = "Mavlink(1.1)",
                }
            );
            d.Info.Add(
                new HeadlinedViewModel("prop2")
                {
                    Icon = MaterialIconKind.MergeType,
                    Header = "Type",
                    Description = "Copter",
                }
            );
            d.Info.Add(
                new HeadlinedViewModel("prop3")
                {
                    Icon = MaterialIconKind.SerialPort,
                    Header = "Port",
                    Description = "serial 1",
                }
            );

            for (int j = 0; j < 5; j++)
            {
                d.Actions.Add(
                    new ActionViewModel($"cmd{i}")
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
    public HomePageViewModel(ICommandService cmd, IAppInfo appInfo)
        : base(PageId, cmd)
    {
        AppInfo = appInfo;
        Icon.Value = MaterialIconKind.Home;
        Title.Value = "Home";

        Tools = [];
        Tools.ObserveRoutableParent(this).DisposeItWith(Disposable);
        ToolsView = Tools.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);

        Items = [];
        Items.ObserveRoutableParent(this).DisposeItWith(Disposable);
        ItemsList = Items.CreateView(x => new HomePageItemViewModel(x)).DisposeItWith(Disposable);
        ItemsList.SetRoutableParentForView(this, true).DisposeItWith(Disposable);

        ItemsView = ItemsList.ToNotifyCollectionChanged().DisposeItWith(Disposable);
    }

    public IAppInfo AppInfo { get; }
    public NotifyCollectionChangedSynchronizedViewList<HomePageItemViewModel> ItemsView { get; }

    public ISynchronizedView<IHomePageItem, HomePageItemViewModel> ItemsList { get; }

    public ObservableList<IHomePageItem> Items { get; }

    public NotifyCollectionChangedSynchronizedViewList<IActionViewModel> ToolsView { get; }

    public ObservableList<IActionViewModel> Tools { get; }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return new ValueTask<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        foreach (var model in Tools)
        {
            yield return model;
        }

        foreach (var model in ItemsView)
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
