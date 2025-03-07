﻿using System.Collections.Immutable;
using System.Composition;
using System.Composition.Hosting;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public abstract class TreePageViewModel<TContext> : PageViewModel<TContext>, IDesignTimeTreePage
    where TContext : class, IPage
{
    private readonly IContainerHost _container;
    private readonly IDisposable _sub2;
    private readonly ObservableList<BreadCrumbItem> _breadCrumbSource;
    private bool _internalNavigate;

    public TreePageViewModel(NavigationId id, ICommandService cmd, IContainerHost container)
        : base(id, cmd)
    {
        _container = container;
        Nodes = new ObservableList<ITreePage>();
        TreeView = new TreePageMenu(Nodes);
        SelectedNode = new BindableReactiveProperty<ObservableTreeNode<ITreePage, NavigationId>?>();
        SelectedPage = new BindableReactiveProperty<IRoutable?>();
        _breadCrumbSource = new ObservableList<BreadCrumbItem>();
        BreadCrumb = _breadCrumbSource.ToViewList();
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

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        if (SelectedPage.CurrentValue != null)
        {
            yield return SelectedPage.CurrentValue;
        }
    }

    protected virtual ISettingsSubPage? CreateSubPage(NavigationId id)
    {
        if (_container.TryGetExport<ISettingsSubPage>(id.Id, out var page))
        {
            page.InitArgs(id.Args);
            return page;
        }

        return null;
    }

    public ObservableTree<ITreePage, NavigationId> TreeView { get; }
    public BindableReactiveProperty<IRoutable?> SelectedPage { get; }
    public ISynchronizedViewList<BreadCrumbItem> BreadCrumb { get; }
    public BindableReactiveProperty<ObservableTreeNode<
        ITreePage,
        NavigationId
    >?> SelectedNode { get; }
    public ObservableList<ITreePage> Nodes { get; }
    public BindableReactiveProperty<bool> IsCompactMode { get; } = new();

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
            _sub2.Dispose();
            IsCompactMode.Dispose();
            SelectedNode.Dispose();
            SelectedPage.Dispose();
            BreadCrumb.Dispose();
            TreeView.Dispose();
        }

        base.Dispose(disposing);
    }
}
