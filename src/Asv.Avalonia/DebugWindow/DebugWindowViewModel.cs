using System.Composition;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

[Export(typeof(IDebugWindow))]
public class DebugWindowViewModel : ViewModelBase, IDebugWindow
{
    public const string ModelId = "DebugWindow";
    private readonly ISynchronizedView<IPage, DebugPageViewModel> _pageView;

    public DebugWindowViewModel()
        : this(DesignTime.Shell) { }

    [ImportingConstructor]
    public DebugWindowViewModel(IShellHost host)
        : base(ModelId)
    {
        SelectedControlPath = host.Shell.SelectedControlPath.ToReadOnlyBindableReactiveProperty([]);
        _pageView = host.Shell.Pages.CreateView(x => new DebugPageViewModel(x));
        Pages = _pageView.ToNotifyCollectionChanged();
        BackwardStack = host.Shell.BackwardStack.ToNotifyCollectionChanged();
        ForwardStack = host.Shell.ForwardStack.ToNotifyCollectionChanged();
    }

    public NotifyCollectionChangedSynchronizedViewList<string[]> ForwardStack { get; }

    public NotifyCollectionChangedSynchronizedViewList<string[]> BackwardStack { get; }

    public NotifyCollectionChangedSynchronizedViewList<DebugPageViewModel> Pages { get; }
    public IReadOnlyBindableReactiveProperty<string[]> SelectedControlPath { get; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _pageView.Dispose();
        }
    }
}

public class DebugPageViewModel : ViewModelBase
{
    public DebugPageViewModel(IPage page)
        : base(page.Id)
    {
        UndoStack = page.History.UndoStack.ToNotifyCollectionChanged();
        RedoStack = page.History.RedoStack.ToNotifyCollectionChanged();
    }

    public NotifyCollectionChangedSynchronizedViewList<HistoryItem> RedoStack { get; }

    public NotifyCollectionChangedSynchronizedViewList<HistoryItem> UndoStack { get; }

    protected override void Dispose(bool disposing)
    {
        // TODO: Implement Dispose
    }
}
