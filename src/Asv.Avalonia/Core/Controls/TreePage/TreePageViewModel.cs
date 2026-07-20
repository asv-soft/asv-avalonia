using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public abstract class TreePageViewModel<TContext, TSubPage>
    : PageViewModel<TContext>,
        ITreePageViewModel
    where TContext : class, ITreePageViewModel
    where TSubPage : ITreeSubpage
{
    private readonly ReactiveProperty<ITreeSubpage?> _selectedPage;
    private readonly IServiceProvider _container;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ObservableList<BreadCrumbItem> _breadCrumbSource;
    private bool _internalNavigate;
    private bool _isLayoutLoaded;

    protected TreePageViewModel(
        string typeId,
        IPageContext context,
        IServiceProvider container,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext
    )
        : base(typeId, context, loggerFactory, dialogService, ext)
    {
        _container = container;
        _loggerFactory = loggerFactory;
        Nodes = [];
        Nodes.SetParent(this).AddTo(ref DisposableBag);
        Nodes.DisposeRemovedItems().AddTo(ref DisposableBag);
        TreeView = new TreePageMenu(Nodes).AddTo(ref DisposableBag);
        SelectedNode = new BindableReactiveProperty<ObservableTreeNode<
            ITreePageMenuItem,
            NavId
        >?>().AddTo(ref DisposableBag);
        _selectedPage = new ReactiveProperty<ITreeSubpage?>().AddTo(ref DisposableBag);
        SelectedPage = _selectedPage.ToBindableReactiveProperty().AddTo(ref DisposableBag);
        _breadCrumbSource = [];
        BreadCrumb = _breadCrumbSource.ToViewList().AddTo(ref DisposableBag);
        SelectedNode.SubscribeAwait(SelectedNodeChanged).AddTo(ref DisposableBag);
        ShowMenuCommand = new ReactiveCommand(_ => ShowMenu(true)).AddTo(ref DisposableBag);
        HideMenuCommand = new ReactiveCommand(_ => ShowMenu(false)).AddTo(ref DisposableBag);
        R3.Disposable.Create(() => SelectedPage.Value?.Dispose()).AddTo(ref DisposableBag);
    }

    public MaterialIconKind? TreeHeaderIcon
    {
        get;
        set => SetField(ref field, value);
    }

    public string? TreeHeader
    {
        get;
        set => SetField(ref field, value);
    }

    public ReactiveCommand ShowMenuCommand { get; }

    public ReactiveCommand HideMenuCommand { get; }

    public bool IsMenuVisible
    {
        get;
        set => SetField(ref field, value);
    } = true;

    public ObservableTree<ITreePageMenuItem, NavId> TreeView { get; }

    public BindableReactiveProperty<ITreeSubpage?> SelectedPage { get; }

    public ISynchronizedViewList<BreadCrumbItem> BreadCrumb { get; }

    public BindableReactiveProperty<ObservableTreeNode<
        ITreePageMenuItem,
        NavId
    >?> SelectedNode { get; }

    public ObservableList<ITreePageMenuItem> Nodes { get; }

    public override ValueTask<IViewModel> Navigate(NavId id, CancellationToken cancel = default)
    {
        try
        {
            if (cancel.IsCancellationRequested)
            {
                return ValueTask.FromResult<IViewModel>(this);
            }

            if (SelectedPage.Value != null && SelectedPage.Value.Id == id)
            {
                return ValueTask.FromResult<IViewModel>(SelectedPage.Value);
            }

            if (SelectedNode.Value?.Base.NavigateTo != id)
            {
                _internalNavigate = true;
                SelectedNode.Value = TreeView.FindNode(x => x.Base.NavigateTo == id);
                _internalNavigate = false;
            }

            var newPage = CreateSubPage(id) ?? CreateDefaultPage();
            if (newPage is null)
            {
                return ValueTask.FromResult<IViewModel>(this);
            }

            var sub = _selectedPage.Value;
            newPage.SetParent(this);
            _selectedPage.Value = newPage;

            var children = _selectedPage.Value.GetChildren();
            foreach (var child in children)
            {
                child.SetParent(newPage);
            }

            sub?.Dispose();
            return ValueTask.FromResult<IViewModel>(newPage);
        }
        catch (Exception exception)
        {
            return ValueTask.FromException<IViewModel>(exception);
        }
    }

    public override IEnumerable<IViewModel> GetChildren()
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

    protected override void AfterLoadExtensions()
    {
        var selectedNodeLayout = Layout.Register<string>(
            nameof(SelectedNode),
            (layoutValue, _) =>
            {
                LoadLayout(layoutValue);
                return ValueTask.CompletedTask;
            }
        );
        var selectedNodeLayoutSave = SelectedNode
            .Skip(1)
            .WhereNotNull()
            .Where(_ => _isLayoutLoaded)
            .SubscribeAwait(
                (node, cancel) => selectedNodeLayout.SaveAsync(node.Key.ToString(), cancel),
                AwaitOperation.Drop
            );
        R3.Disposable.Combine(selectedNodeLayout, selectedNodeLayoutSave).AddTo(ref DisposableBag);

        var isMenuVisibleLayout = Layout.Register<bool>(
            nameof(IsMenuVisible),
            (value, _) =>
            {
                IsMenuVisible = value;
                return ValueTask.CompletedTask;
            }
        );
        var isMenuVisibleLayoutSave = this.ObservePropertyChanged(x => x.IsMenuVisible)
            .Where(_ => _isLayoutLoaded)
            .SubscribeAwait(
                (_, cancel) => isMenuVisibleLayout.SaveAsync(IsMenuVisible, cancel),
                AwaitOperation.Drop
            );
        R3.Disposable.Combine(isMenuVisibleLayout, isMenuVisibleLayoutSave)
            .AddTo(ref DisposableBag);

        RootTracking.ExecuteWhenRootAttached(LoadLayoutWhenRootAttached).AddTo(ref DisposableBag);
        return;

        async ValueTask LoadLayoutWhenRootAttached(IShell root, CancellationToken cancel)
        {
            _ = root;
            _isLayoutLoaded = false;
            try
            {
                await selectedNodeLayout.LoadAsync(cancel);
                await isMenuVisibleLayout.LoadAsync(cancel);
            }
            finally
            {
                if (!cancel.IsCancellationRequested && !IsDisposed)
                {
                    _isLayoutLoaded = true;
                }
            }
        }
    }

    private void LoadLayout(string layoutValue)
    {
        var id = NavId.Parse(layoutValue);
        SelectedNode.Value = TreeView.FindNode(x => x.Key == id || x.Base.NavigateTo == id);
    }

    protected virtual ITreeSubpage? CreateDefaultPage()
    {
        return SelectedNode.Value != null
            ? new GroupTreePageItemViewModel(SelectedNode.Value, Navigate)
            : null;
    }

    protected virtual ITreeSubpage? CreateSubPage(NavId id)
    {
        try
        {
            var context = new TreeSubPageContext<TContext>(id.Args, Context);
            return _container.CreateTreeSubPage<TContext, TSubPage>(id.TypeId, context);
        }
        catch (Exception e)
        {
            return null;
        }
    }

    private void ShowMenu(bool value)
    {
        IsMenuVisible = value;
    }

    private async ValueTask SelectedNodeChanged(
        ObservableTreeNode<ITreePageMenuItem, NavId>? node,
        CancellationToken cancel
    )
    {
        if (cancel.IsCancellationRequested)
        {
            return;
        }

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

        await Navigate(node.Base.NavigateTo, cancel);
    }
}
