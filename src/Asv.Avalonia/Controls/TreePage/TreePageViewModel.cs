using Asv.Common;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class TreePageViewModelConfig
{
    public string SelectedNodeId { get; set; } = string.Empty;
}

public abstract class TreePageViewModel<TContext, TSubPage>
    : PageViewModel<TContext>,
        ITreePageViewModel
    where TContext : class, IPage
    where TSubPage : ITreeSubpage<TContext>
{
    private readonly ILayoutService _layoutService;
    private readonly ReactiveProperty<ITreeSubpage?> _selectedPage;
    private readonly IContainerHost _container;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ObservableList<BreadCrumbItem> _breadCrumbSource;
    private bool _internalNavigate;
    private TreePageViewModelConfig? _config;

    protected TreePageViewModel(
        NavigationId id,
        ICommandService cmd,
        IContainerHost container,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory
    )
        : base(id, cmd, loggerFactory)
    {
        _container = container;
        _loggerFactory = loggerFactory;
        _layoutService = layoutService;
        Nodes = [];
        Nodes.SetRoutableParent(this).DisposeItWith(Disposable);
        Nodes.DisposeRemovedItems().DisposeItWith(Disposable);
        TreeView = new TreePageMenu(Nodes).DisposeItWith(Disposable);
        SelectedNode = new BindableReactiveProperty<ObservableTreeNode<
            ITreePage,
            NavigationId
        >?>().DisposeItWith(Disposable);
        _selectedPage = new ReactiveProperty<ITreeSubpage?>().DisposeItWith(Disposable);
        SelectedPage = _selectedPage.ToBindableReactiveProperty().DisposeItWith(Disposable);
        _breadCrumbSource = [];
        BreadCrumb = _breadCrumbSource.ToViewList().DisposeItWith(Disposable);
        SelectedNode.SubscribeAwait(SelectedNodeChanged).DisposeItWith(Disposable);
        ShowMenuCommand = new ReactiveCommand(_ => ShowMenu(true)).DisposeItWith(Disposable);
        HideMenuCommand = new ReactiveCommand(_ => ShowMenu(false)).DisposeItWith(Disposable);

        _selectedPage
            .WhereNotNull()
            .SubscribeAwait(async (p, ct) => await p.RequestLoadLayout(layoutService, ct))
            .DisposeItWith(Disposable);
    }

    #region Menu

    public ReactiveCommand ShowMenuCommand { get; }
    public ReactiveCommand HideMenuCommand { get; }

    private void ShowMenu(bool value)
    {
        IsMenuVisible = value;
    }

    public bool IsMenuVisible
    {
        get;
        set => SetField(ref field, value);
    } = true;

    #endregion

    private async ValueTask SelectedNodeChanged(
        ObservableTreeNode<ITreePage, NavigationId>? node,
        CancellationToken cancel
    )
    {
        if (node?.Base.NavigateTo is null || _internalNavigate)
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

        await this.RequestSaveLayout(_layoutService, cancel);
        await this.RequestSaveLayoutToFile(_layoutService, cancel, RoutingStrategy.Direct);
        await Navigate(node.Base.NavigateTo);
    }

    protected virtual ITreeSubpage? CreateDefaultPage()
    {
        return SelectedNode.Value != null
            ? new GroupTreePageItemViewModel(SelectedNode.Value, Navigate, _loggerFactory)
            : null;
    }

    public override async ValueTask<IRoutable> Navigate(NavigationId id)
    {
        if (SelectedPage.Value != null && SelectedPage.Value.Id == id)
        {
            return SelectedPage.Value;
        }

        if (SelectedNode.Value?.Base.NavigateTo != id)
        {
            _internalNavigate = true;
            SelectedNode.Value = TreeView.FindNode(x => x.Base.NavigateTo == id);
            _internalNavigate = false;
        }

        var newPage = await CreateSubPage(id) ?? CreateDefaultPage();
        if (newPage is null)
        {
            return this;
        }

        var sub = _selectedPage.Value;
        newPage.Parent = this;
        _selectedPage.Value = newPage;

        var children = _selectedPage.Value.GetRoutableChildren();
        foreach (var child in children)
        {
            child.Parent = newPage;
        }

        sub?.Dispose();
        return newPage;
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        foreach (var node in Nodes)
        {
            yield return node;
        }

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

    protected override ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        switch (e)
        {
            case PageCloseRequestedEvent close:
            {
                if (Id != close.Page.Id)
                {
                    break;
                }

                foreach (var node in Nodes)
                {
                    _layoutService.RemoveFromMemoryViewModelAndView(node);
                }

                break;
            }
            case SaveLayoutEvent saveLayoutEvent:
                if (_config is null)
                {
                    break;
                }

                this.HandleSaveLayout(
                    saveLayoutEvent,
                    _config,
                    cfg => cfg.SelectedNodeId = SelectedNode.Value?.Key.ToString() ?? string.Empty
                );
                break;
            case LoadLayoutEvent loadLayoutEvent:
                _config = this.HandleLoadLayout<TreePageViewModelConfig>(
                    loadLayoutEvent,
                    cfg =>
                    {
                        if (!string.IsNullOrEmpty(cfg.SelectedNodeId))
                        {
                            SetSelectedNodeFromConfig(cfg);
                        }
                    }
                );

                break;
        }

        return base.InternalCatchEvent(e);
    }

    private void SetSelectedNodeFromConfig(TreePageViewModelConfig cfg)
    {
        var selectedNode = TreeView.FindNode(x => x.Base.NavigateTo == cfg.SelectedNodeId);

        SelectedNode.Value = selectedNode;

        if (selectedNode is not null)
        {
            return;
        }

        // TreeView may not have all nodes yet
        _sub1?.Dispose();
        _sub1 = null;
        _sub1 = Nodes
            .ObserveAdd()
            .Subscribe(addEvent =>
            {
                if (addEvent.Value.Id != cfg.SelectedNodeId)
                {
                    return;
                }

                selectedNode = TreeView.FindNode(x => x.Base.NavigateTo == cfg.SelectedNodeId);
                if (selectedNode is null)
                {
                    return;
                }

                SelectedNode.Value = selectedNode;
                _sub1?.Dispose();
            });
    }

    public ObservableTree<ITreePage, NavigationId> TreeView { get; }
    public BindableReactiveProperty<ITreeSubpage?> SelectedPage { get; }
    public ISynchronizedViewList<BreadCrumbItem> BreadCrumb { get; }

    public BindableReactiveProperty<ObservableTreeNode<
        ITreePage,
        NavigationId
    >?> SelectedNode { get; }
    public ObservableList<ITreePage> Nodes { get; }

    protected override TContext GetContext()
    {
        return this as TContext ?? throw new InvalidOperationException("Can't cast to context");
    }

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }

    #region Dispose

    private IDisposable? _sub1;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1?.Dispose();
            SelectedPage.Value?.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
