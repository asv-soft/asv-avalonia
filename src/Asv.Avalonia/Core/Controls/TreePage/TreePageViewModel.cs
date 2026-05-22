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
        Nodes.SetRoutableParent(this).AddTo(ref DisposableBag);
        Nodes.DisposeRemovedItems().AddTo(ref DisposableBag);
        TreeView = new TreePageMenu(Nodes).AddTo(ref DisposableBag);
        SelectedNode = new BindableReactiveProperty<ObservableTreeNode<ITreePage, NavId>?>().AddTo(
            ref DisposableBag
        );
        _selectedPage = new ReactiveProperty<ITreeSubpage?>().AddTo(ref DisposableBag);
        SelectedPage = _selectedPage.ToBindableReactiveProperty().AddTo(ref DisposableBag);
        _breadCrumbSource = [];
        BreadCrumb = _breadCrumbSource.ToViewList().AddTo(ref DisposableBag);
        SelectedNode.SubscribeAwait(SelectedNodeChanged).AddTo(ref DisposableBag);
        ShowMenuCommand = new ReactiveCommand(_ => ShowMenu(true)).AddTo(ref DisposableBag);
        HideMenuCommand = new ReactiveCommand(_ => ShowMenu(false)).AddTo(ref DisposableBag);
        R3.Disposable.Create(() => SelectedPage.Value?.Dispose()).AddTo(ref DisposableBag);
        
        Layout.Register(nameof(SelectedNode), LoadLayout, SaveLayout, SelectedNode)
            .AddTo(ref DisposableBag);
        Layout
            .Register(
                nameof(IsMenuVisible),
                x => IsMenuVisible = x,
                () => IsMenuVisible,
                this.ObservePropertyChanged(x => x.IsMenuVisible)
            )
            .AddTo(ref DisposableBag);

        Layout.LoadWhenRootAttached(RootTracking).DisposeItWith(Disposable);
        
        
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

    public ObservableTree<ITreePage, NavId> TreeView { get; }

    public BindableReactiveProperty<ITreeSubpage?> SelectedPage { get; }

    public ISynchronizedViewList<BreadCrumbItem> BreadCrumb { get; }

    public BindableReactiveProperty<ObservableTreeNode<ITreePage, NavId>?> SelectedNode { get; }

    public ObservableList<ITreePage> Nodes { get; }

    public override ValueTask<IViewModel> Navigate(NavId id)
    {
        try
        {
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
            newPage.Layout.LoadAllAsync(CancellationToken.None).SafeFireAndForget();

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
        // do nothing
    }

    private string? SaveLayout()
    {
        return SelectedNode.Value?.Key.ToString();
    }

    private void LoadLayout(string layoutValue)
    {
        Navigate(NavId.Parse(layoutValue)).SafeFireAndForget();
    }

    protected virtual ITreeSubpage? CreateDefaultPage()
    {
        return SelectedNode.Value != null
            ? new GroupTreePageItemViewModel(SelectedNode.Value, Navigate, _loggerFactory)
            : null;
    }

    protected virtual ITreeSubpage CreateSubPage(NavId id)
    {
        var context = new TreeSubPageContext<TContext>(id.Args, Context);
        return _container.CreateTreeSubPage<TContext, TSubPage>(id.TypeId, context);
    }

    private void ShowMenu(bool value)
    {
        IsMenuVisible = value;
    }

    private async ValueTask SelectedNodeChanged(
        ObservableTreeNode<ITreePage, NavId>? node,
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

        await Navigate(node.Base.NavigateTo);
    }
}
