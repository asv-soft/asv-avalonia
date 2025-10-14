using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public interface IDesignTimeTreePage : IPage
{
    ObservableTree<ITreePage, NavigationId> TreeView { get; }
    BindableReactiveProperty<ObservableTreeNode<ITreePage, NavigationId>?> SelectedNode { get; }
    BindableReactiveProperty<ITreeSubpage?> SelectedPage { get; }
    ISynchronizedViewList<BreadCrumbItem> BreadCrumb { get; }
    bool IsMenuVisible { get; }
    ReactiveCommand ShowMenuCommand { get; }
    ReactiveCommand HideMenuCommand { get; }
}

public class DesignTimeTreePageViewModel : TreePageViewModel<IPage, ITreeSubpage<IPage>>
{
    public DesignTimeTreePageViewModel()
        : base(
            DesignTime.Id,
            DesignTime.CommandService,
            DesignTime.ContainerHost,
            NullLayoutService.Instance,
            DesignTime.LoggerFactory
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        Nodes.Add(
            new TreePage(
                "node1",
                "Node1",
                MaterialIconKind.Abacus,
                "node1",
                NavigationId.Empty,
                NullLayoutService.Instance,
                DesignTime.LoggerFactory
            )
        );
        Nodes.Add(
            new TreePage(
                "node2",
                "node2",
                MaterialIconKind.Abacus,
                "node2",
                NavigationId.Empty,
                NullLayoutService.Instance,
                DesignTime.LoggerFactory
            )
        );
        Nodes.Add(
            new TreePage(
                "node3",
                "node3",
                MaterialIconKind.Abacus,
                "node3",
                NavigationId.Empty,
                NullLayoutService.Instance,
                DesignTime.LoggerFactory
            )
        );
        Nodes.Add(
            new TreePage(
                "node4",
                "node4",
                MaterialIconKind.Abacus,
                "node4",
                NavigationId.Empty,
                NullLayoutService.Instance,
                DesignTime.LoggerFactory
            )
        );
        Nodes.Add(
            new TreePage(
                "node5",
                "node5",
                MaterialIconKind.Abacus,
                "node5",
                NavigationId.Empty,
                NullLayoutService.Instance,
                DesignTime.LoggerFactory
            )
        );
        Nodes.Add(
            new TreePage(
                "node1.1",
                "node1.1",
                MaterialIconKind.Abacus,
                "node1",
                "node1",
                NullLayoutService.Instance,
                DesignTime.LoggerFactory
            )
        );
        Nodes.Add(
            new TreePage(
                "node1.2",
                "node1.2",
                MaterialIconKind.Abacus,
                "node1",
                "node1",
                NullLayoutService.Instance,
                DesignTime.LoggerFactory
            )
        );
        Nodes.Add(
            new TreePage(
                "node1.3",
                "node1.3",
                MaterialIconKind.Abacus,
                "node1",
                "node1",
                NullLayoutService.Instance,
                DesignTime.LoggerFactory
            )
        );
        Nodes.Add(
            new TreePage(
                "node1.4",
                "node1.4",
                MaterialIconKind.Abacus,
                "node1",
                "node1",
                NullLayoutService.Instance,
                DesignTime.LoggerFactory
            )
        );
        Nodes.Add(
            new TreePage(
                "node1.5",
                "node1.5",
                MaterialIconKind.Abacus,
                "node1",
                "node1",
                NullLayoutService.Instance,
                DesignTime.LoggerFactory
            )
        );
        Nodes.Add(
            new TreePage(
                "node1.1.1",
                "node1.1.1",
                MaterialIconKind.Abacus,
                "node1.1",
                "node1.1",
                NullLayoutService.Instance,
                DesignTime.LoggerFactory
            )
        );
        Nodes.Add(
            new TreePage(
                "node1.1.2",
                "node1.1.2",
                MaterialIconKind.Abacus,
                "node1.1",
                "node1.1",
                NullLayoutService.Instance,
                DesignTime.LoggerFactory
            )
        );

        Init();
    }

    protected override ValueTask<ITreeSubpage?> CreateSubPage(NavigationId id)
    {
        var set = new SettingsAppearanceViewModel();
        set.Menu.Add(
            new MenuItem("cmd0", "Command", NullLayoutService.Instance, DesignTime.LoggerFactory)
        );
        set.Menu.Add(
            new MenuItem("cmd01", "Command1", NullLayoutService.Instance, DesignTime.LoggerFactory)
        );
        set.Menu.Add(
            new MenuItem("cmd02", "Command2", NullLayoutService.Instance, DesignTime.LoggerFactory)
        );
        set.Menu.Add(
            new MenuItem("cmd03", "Command3", NullLayoutService.Instance, DesignTime.LoggerFactory)
        );
        return ValueTask.FromResult<ITreeSubpage?>(set);
    }

    public override IExportInfo Source => SystemModule.Instance;
}
