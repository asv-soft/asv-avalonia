using System.Collections.Immutable;
using System.Composition;
using System.Composition.Hosting;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class TreePageViewModel<TContext> : PageViewModel<TContext>, IDesignTimeTreePage
    where TContext : class, IPage
{
    private readonly IContainerHost _container;
    private readonly IDisposable _sub2;
    private readonly ObservableList<BreadCrumbItem> _breadCrumbSource;
    private bool _internalNavigate;

    public TreePageViewModel(string id, ICommandService cmd, IContainerHost container)
        : base(id, cmd)
    {
        _container = container;
        Nodes = new ObservableList<ITreePageNode>();
        SelectedNode = new BindableReactiveProperty<ObservableTreeNode<ITreePageNode, string>?>();
        SelectedPage = new BindableReactiveProperty<IRoutable?>();
        _breadCrumbSource = new ObservableList<BreadCrumbItem>();
        BreadCrumb = _breadCrumbSource.ToViewList();
        Tree = Nodes.ToObservableTree(x => x.Id, x => x.ParentId);
        _sub2 = SelectedNode.SubscribeAwait(
            async (x, _) =>
            {
                if (x?.Base.NavigateTo == null || _internalNavigate)
                {
                    return;
                }

                _breadCrumbSource.Clear();
                if (SelectedNode.Value != null)
                {
                    _breadCrumbSource.AddRange(
                        SelectedNode
                            .Value.GetAllMenuFromRoot()
                            .Select((item, index) => new BreadCrumbItem(index == 0, item.Base))
                    );
                }

                await Navigate(x.Base.NavigateTo);
            }
        );
    }

    public override async ValueTask<IRoutable> Navigate(string id)
    {
        if (SelectedPage.Value != null && SelectedPage.Value.Id == id)
        {
            await ValueTask.FromResult(SelectedPage.Value);
        }

        if (SelectedNode.Value?.Base.NavigateTo != id)
        {
            _internalNavigate = true;
            SelectedNode.Value = Tree.FindNode(x => x.Base.NavigateTo == id);
            _internalNavigate = false;
        }

        var newPage = CreateSubPage(id);
        if (newPage == null)
        {
            return this;
        }

        SelectedPage.Value?.Dispose();
        newPage.Parent = this;
        SelectedPage.Value = newPage;
        return newPage;
    }

    protected virtual ISettingsSubPage? CreateSubPage(string id)
    {
        return _container.TryGetExport<ISettingsSubPage>(id, out var page) ? page : null;
    }

    public ObservableTree<ITreePageNode, string> Tree { get; }
    public BindableReactiveProperty<IRoutable?> SelectedPage { get; }
    public ISynchronizedViewList<BreadCrumbItem> BreadCrumb { get; }
    public BindableReactiveProperty<ObservableTreeNode<
        ITreePageNode,
        string
    >?> SelectedNode { get; }
    public ObservableList<ITreePageNode> Nodes { get; }
    public BindableReactiveProperty<bool> IsCompactMode { get; } = new();

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub2.Dispose();
            IsCompactMode.Dispose();
            SelectedNode.Dispose();
            SelectedPage.Dispose();
            BreadCrumb.Dispose();
            Tree.Dispose();
        }

        base.Dispose(disposing);
    }
}
