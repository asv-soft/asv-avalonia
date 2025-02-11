using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public interface IDesignTimeTreePage : IPage
{
    BindableReactiveProperty<bool> IsCompactMode { get; }
    ObservableTree<ITreePageNode, string> Tree { get; }
    BindableReactiveProperty<ObservableTreeNode<ITreePageNode, string>?> SelectedNode { get; }
    BindableReactiveProperty<IRoutable?> SelectedPage { get; }
    ISynchronizedViewList<BreadCrumbItem> BreadCrumb { get; }
}

public class DesignTimeTreePageViewModel : TreePageViewModel<IPage>
{
    public DesignTimeTreePageViewModel()
        : base(DesignTime.Id, DesignTime.CommandService, DesignTime.ContainerHost)
    {
        DesignTime.ThrowIfNotDesignMode();
        Nodes.Add(new TreePageNode("node1", "node1", "node1"));
        Nodes.Add(new TreePageNode("node2", "node2", "node2"));
        Nodes.Add(new TreePageNode("node3", "node3", "node3"));
        Nodes.Add(new TreePageNode("node4", "node4", "node4"));
        Nodes.Add(new TreePageNode("node5", "node5", "node5"));
        Nodes.Add(new TreePageNode("node1.1", "node1.1", "node1", "node1"));
        Nodes.Add(new TreePageNode("node1.2", "node1.2", "node1", "node1"));
        Nodes.Add(new TreePageNode("node1.3", "node1.3", "node1", "node1"));
        Nodes.Add(new TreePageNode("node1.4", "node1.4", "node1", "node1"));
        Nodes.Add(new TreePageNode("node1.5", "node1.5", "node1", "node1"));
        Nodes.Add(new TreePageNode("node1.1.1", "node1.1.1", "node1.1", "node1.1"));
        Nodes.Add(new TreePageNode("node1.1.2", "node1.1.2", "node1.1", "node1.1"));

        Init();
    }

    protected override ISettingsSubPage? CreateSubPage(string id)
    {
        return new SettingsAppearanceViewModel();
    }
}
