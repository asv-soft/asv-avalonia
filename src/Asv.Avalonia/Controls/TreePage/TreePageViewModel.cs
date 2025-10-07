using Asv.Cfg;
using Asv.Common;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public abstract class TreePageViewModel<TContext, TSubPage, TConfig>
    : PageViewModel<TContext, TConfig>,
        ITreePageViewModel
    where TContext : class, IPage
    where TSubPage : ITreeSubpage<TContext>
    where TConfig : PageConfig, new()
{
    private readonly ReactiveProperty<ITreeSubpage?> _selectedPage;
    private readonly IContainerHost _container;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ObservableList<BreadCrumbItem> _breadCrumbSource;
    private bool _internalNavigate;

    protected TreePageViewModel(
        NavigationId id,
        ICommandService cmd,
        IContainerHost container,
        IConfiguration cfg,
        ILoggerFactory loggerFactory
    )
        : base(id, cmd, cfg, loggerFactory)
    {
        _container = container;
        _loggerFactory = loggerFactory;
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
        if (newPage == null)
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
