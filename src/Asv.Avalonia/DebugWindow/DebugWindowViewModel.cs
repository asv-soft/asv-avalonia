using System.Composition;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

[Export(typeof(IDebugWindow))]
public class DebugWindowViewModel : ViewModelBase, IDebugWindow
{
    public const string ModelId = "DebugWindow";
    private readonly ISynchronizedView<IPage, DebugPageViewModel> _pageView;

    public DebugWindowViewModel()
        : this(
            DesignTime.Navigation,
            DesignTime.ShellHost,
            DesignTime.CommandService,
            NullLayoutService.Instance,
            DesignTime.LoggerFactory
        ) { }

    [ImportingConstructor]
    public DebugWindowViewModel(
        INavigationService nav,
        IShellHost host,
        ICommandService cmd,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory
    )
        : base(ModelId, loggerFactory)
    {
        SelectedControlPath = nav.SelectedControlPath.ToReadOnlyBindableReactiveProperty();
        _pageView = host.Shell.Pages.CreateView(x => new DebugPageViewModel(x, loggerFactory));
        Pages = _pageView.ToNotifyCollectionChanged();
        BackwardStack = nav.BackwardStack.ToNotifyCollectionChanged();
        ForwardStack = nav.ForwardStack.ToNotifyCollectionChanged();
        HotKey = cmd.OnHotKey.ToReadOnlyBindableReactiveProperty();
    }

    public NotifyCollectionChangedSynchronizedViewList<NavigationPath> ForwardStack { get; }

    public NotifyCollectionChangedSynchronizedViewList<NavigationPath> BackwardStack { get; }

    public NotifyCollectionChangedSynchronizedViewList<DebugPageViewModel> Pages { get; }
    public IReadOnlyBindableReactiveProperty<NavigationPath> SelectedControlPath { get; }

    public IReadOnlyBindableReactiveProperty<HotKeyInfo> HotKey { get; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _pageView.Dispose();
        }
    }
}

public class DebugPageViewModel(IPage page, ILoggerFactory loggerFactory)
    : ViewModelBase(page.Id, loggerFactory)
{
    public NotifyCollectionChangedSynchronizedViewList<CommandSnapshot> RedoStack { get; } =
        page.History.RedoStack.ToNotifyCollectionChanged();

    public NotifyCollectionChangedSynchronizedViewList<CommandSnapshot> UndoStack { get; } =
        page.History.UndoStack.ToNotifyCollectionChanged();

    protected override void Dispose(bool disposing)
    {
        // TODO: Implement Dispose
    }
}
