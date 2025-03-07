using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public interface IDesignTimeTreePage : IPage
{
    BindableReactiveProperty<bool> IsCompactMode { get; }
    ObservableTree<ITreePage, NavigationId> TreeView { get; }
    BindableReactiveProperty<ObservableTreeNode<ITreePage, NavigationId>?> SelectedNode { get; }
    BindableReactiveProperty<IRoutable?> SelectedPage { get; }
    ISynchronizedViewList<BreadCrumbItem> BreadCrumb { get; }
}

public class DesignTimeTreePageViewModel : TreePageViewModel<IPage>
{
    public DesignTimeTreePageViewModel()
        : base(DesignTime.Id, DesignTime.CommandService, DesignTime.ContainerHost)
    {
        DesignTime.ThrowIfNotDesignMode();
        Nodes.Add(
            new TreePage("node1", "Node1", MaterialIconKind.Abacus, "node1", NavigationId.Empty)
        );
        Nodes.Add(
            new TreePage("node2", "node2", MaterialIconKind.Abacus, "node2", NavigationId.Empty)
        );
        Nodes.Add(
            new TreePage("node3", "node3", MaterialIconKind.Abacus, "node3", NavigationId.Empty)
        );
        Nodes.Add(
            new TreePage("node4", "node4", MaterialIconKind.Abacus, "node4", NavigationId.Empty)
        );
        Nodes.Add(
            new TreePage("node5", "node5", MaterialIconKind.Abacus, "node5", NavigationId.Empty)
        );
        Nodes.Add(new TreePage("node1.1", "node1.1", MaterialIconKind.Abacus, "node1", "node1"));
        Nodes.Add(new TreePage("node1.2", "node1.2", MaterialIconKind.Abacus, "node1", "node1"));
        Nodes.Add(new TreePage("node1.3", "node1.3", MaterialIconKind.Abacus, "node1", "node1"));
        Nodes.Add(new TreePage("node1.4", "node1.4", MaterialIconKind.Abacus, "node1", "node1"));
        Nodes.Add(new TreePage("node1.5", "node1.5", MaterialIconKind.Abacus, "node1", "node1"));
        Nodes.Add(
            new TreePage("node1.1.1", "node1.1.1", MaterialIconKind.Abacus, "node1.1", "node1.1")
        );
        Nodes.Add(
            new TreePage("node1.1.2", "node1.1.2", MaterialIconKind.Abacus, "node1.1", "node1.1")
        );

        Init();
    }

    protected override ISettingsSubPage? CreateSubPage(NavigationId id)
    {
        return new SettingsAppearanceViewModel();
    }

    public override IExportInfo Source => SystemModule.Instance;
}
