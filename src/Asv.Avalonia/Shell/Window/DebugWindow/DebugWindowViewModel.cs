using System.Diagnostics;
using Asv.Modeling;
using Avalonia.Input;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class DebugWindowViewModel : ViewModel, IDebugWindow
{
    public const string ModelId = "DebugWindow";
    private readonly ISynchronizedView<IPage, DebugPageViewModel> _pageView;

    public DebugWindowViewModel()
        : this(
            DesignTime.ShellHost,
            DesignTime.HotKeyService,
            DesignTime.LoggerFactory
        ) { }

    public DebugWindowViewModel(
        IShellHost host,
        IHotKeyService hotKeys,
        ILoggerFactory loggerFactory
    )
        : base(ModelId)
    {
        Debug.Assert(host.Shell != null, "host.Shell != null");
        SelectedControlPath = host.Shell.Navigation.SelectedPath.ToReadOnlyBindableReactiveProperty();
        _pageView = host.Shell.Pages.CreateView(x => new DebugPageViewModel(x));
        Pages = _pageView.ToNotifyCollectionChanged();
        BackwardStack = host.Shell.Navigation.BackwardStack.ToNotifyCollectionChanged();
        ForwardStack = host.Shell.Navigation.ForwardStack.ToNotifyCollectionChanged();
        HotKey = hotKeys.OnHotKey.ToReadOnlyBindableReactiveProperty();
    }

    public NotifyCollectionChangedSynchronizedViewList<NavPath> ForwardStack { get; }

    public NotifyCollectionChangedSynchronizedViewList<NavPath> BackwardStack { get; }

    public NotifyCollectionChangedSynchronizedViewList<DebugPageViewModel> Pages { get; }
    public IReadOnlyBindableReactiveProperty<NavPath> SelectedControlPath { get; }

    public IReadOnlyBindableReactiveProperty<KeyGesture?> HotKey { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _pageView.Dispose();
        }
    }
}

public class DebugPageViewModel(IPage page)
    : ViewModel(page.Id.TypeId, page.Id.Args)
{
    public NotifyCollectionChangedSynchronizedViewList<IUndoSnapshot> RedoStack { get; } =
        page.UndoHistory.RedoStack.ToNotifyCollectionChanged();

    public NotifyCollectionChangedSynchronizedViewList<IUndoSnapshot> UndoStack { get; } =
        page.UndoHistory.UndoStack.ToNotifyCollectionChanged();

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }

    protected override void Dispose(bool disposing)
    {
        // TODO: Implement Dispose
    }
}
