using System.Diagnostics;
using Asv.Modeling;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class DebugWindowViewModel : ViewModelBase, IDebugWindow
{
    public const string ModelId = "DebugWindow";
    private readonly ISynchronizedView<IPage, DebugPageViewModel> _pageView;

    public DebugWindowViewModel()
        : this(
            DesignTime.Navigation,
            DesignTime.ShellHost,
            DesignTime.CommandService,
            DesignTime.LoggerFactory
        ) { }

    public DebugWindowViewModel(
        INavigationService nav,
        IShellHost host,
        ICommandService cmd,
        ILoggerFactory loggerFactory
    )
        : base(ModelId, default, loggerFactory)
    {
        SelectedControlPath = nav.SelectedControlPath.ToReadOnlyBindableReactiveProperty();
        Debug.Assert(host.Shell != null, "host.Shell != null");
        _pageView = host.Shell.Pages.CreateView(x => new DebugPageViewModel(x, loggerFactory));
        Pages = _pageView.ToNotifyCollectionChanged();
        BackwardStack = nav.BackwardStack.ToNotifyCollectionChanged();
        ForwardStack = nav.ForwardStack.ToNotifyCollectionChanged();
        HotKey = cmd.OnHotKey.ToReadOnlyBindableReactiveProperty();
    }

    public NotifyCollectionChangedSynchronizedViewList<NavPath> ForwardStack { get; }

    public NotifyCollectionChangedSynchronizedViewList<NavPath> BackwardStack { get; }

    public NotifyCollectionChangedSynchronizedViewList<DebugPageViewModel> Pages { get; }
    public IReadOnlyBindableReactiveProperty<NavPath> SelectedControlPath { get; }

    public IReadOnlyBindableReactiveProperty<HotKeyInfo> HotKey { get; }

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

public class DebugPageViewModel(IPage page, ILoggerFactory loggerFactory)
    : ViewModelBase(page.Id.TypeId, page.Id.Args, loggerFactory)
{
    public NotifyCollectionChangedSynchronizedViewList<CommandSnapshot> RedoStack { get; } =
        page.History.RedoStack.ToNotifyCollectionChanged();

    public NotifyCollectionChangedSynchronizedViewList<CommandSnapshot> UndoStack { get; } =
        page.History.UndoStack.ToNotifyCollectionChanged();

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }

    protected override void Dispose(bool disposing)
    {
        // TODO: Implement Dispose
    }
}
