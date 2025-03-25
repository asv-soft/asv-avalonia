using System.Collections.Immutable;
using System.Composition;
using System.Composition.Hosting;
using Asv.Common;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public abstract class TreePageViewModel<TContext, TSubPage>
    : PageViewModel<TContext>,
        IDesignTimeTreePage
    where TContext : class, IPage
    where TSubPage : ITreeSubpage<TContext>
{
    private readonly IContainerHost _container;
    private readonly ObservableList<BreadCrumbItem> _breadCrumbSource;
    private bool _internalNavigate;
    private MenuTree? _pageMenu;

    public TreePageViewModel(NavigationId id, ICommandService cmd, IContainerHost container)
        : base(id, cmd)
    {
        _container = container;
        IsCompactMode = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
        Nodes = [];
        Nodes.SetRoutableParent(this, true);
        TreeView = new TreePageMenu(Nodes).DisposeItWith(Disposable);
        SelectedNode = new BindableReactiveProperty<ObservableTreeNode<
            ITreePage,
            NavigationId
        >?>().DisposeItWith(Disposable);
        SelectedPage = new BindableReactiveProperty<ITreeSubpage?>().DisposeItWith(Disposable);
        _breadCrumbSource = [];
        BreadCrumb = _breadCrumbSource.ToViewList().DisposeItWith(Disposable);
        SelectedNode.SubscribeAwait(SelectedNodeChanged).DisposeItWith(Disposable);
    }

    private async ValueTask SelectedNodeChanged(
        ObservableTreeNode<ITreePage, NavigationId>? node,
        CancellationToken cancel
    )
    {
        if (node?.Base.NavigateTo == null || _internalNavigate)
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

        await Navigate(node.Base.NavigateTo);
    }

    public override async ValueTask<IRoutable> Navigate(NavigationId id)
    {
        if (SelectedPage.Value != null && SelectedPage.Value.Id == id)
        {
            await ValueTask.FromResult(SelectedPage.Value);
        }

        if (SelectedNode.Value?.Base.NavigateTo != id)
        {
            _internalNavigate = true;
            SelectedNode.Value = TreeView.FindNode(x => x.Base.NavigateTo == id);
            _internalNavigate = false;
        }

        var newPage = await CreateSubPage(id);
        if (newPage == null)
        {
            return this;
        }

        if (PageMenu != null)
        {
            await PageMenu.DisposeAsync();
        }

        PageMenu = new MenuTree(newPage.Menu).DisposeItWith(Disposable);

        SelectedPage.Value?.Dispose();
        newPage.Parent = this;
        SelectedPage.Value = newPage;
        return newPage;
    }

    public MenuTree? PageMenu
    {
        get => _pageMenu;
        set => SetField(ref _pageMenu, value);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        if (SelectedPage.CurrentValue != null)
        {
            yield return SelectedPage.CurrentValue;
        }
    }

    protected virtual async ValueTask<ITreeSubpage?> CreateSubPage(NavigationId id)
    {
        if (_container.TryGetExport<TSubPage>(id.Id, out var page))
        {
            page.InitArgs(id.Args);
            await page.Init(GetContext());
            return page;
        }

        return null;
    }

    public ObservableTree<ITreePage, NavigationId> TreeView { get; }
    public BindableReactiveProperty<ITreeSubpage?> SelectedPage { get; }
    public ISynchronizedViewList<BreadCrumbItem> BreadCrumb { get; }
    public BindableReactiveProperty<ObservableTreeNode<
        ITreePage,
        NavigationId
    >?> SelectedNode { get; }
    public ObservableList<ITreePage> Nodes { get; }
    public BindableReactiveProperty<bool> IsCompactMode { get; }

    protected override TContext GetContext()
    {
        return this as TContext ?? throw new InvalidOperationException("Can't cast to context");
    }

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            SelectedPage.Value?.Dispose();
        }

        base.Dispose(disposing);
    }
}
