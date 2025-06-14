﻿using System.Composition;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

[Export(typeof(IDebugWindow))]
public class DebugWindowViewModel : ViewModelBase, IDebugWindow
{
    public const string ModelId = "DebugWindow";
    private readonly ISynchronizedView<IPage, DebugPageViewModel> _pageView;

    public DebugWindowViewModel()
        : this(DesignTime.Navigation, DesignTime.ShellHost, DesignTime.CommandService) { }

    [ImportingConstructor]
    public DebugWindowViewModel(INavigationService nav, IShellHost host, ICommandService cmd)
        : base(ModelId)
    {
        SelectedControlPath = nav.SelectedControlPath.ToReadOnlyBindableReactiveProperty();
        _pageView = host.Shell.Pages.CreateView(x => new DebugPageViewModel(x));
        Pages = _pageView.ToNotifyCollectionChanged();
        BackwardStack = nav.BackwardStack.ToNotifyCollectionChanged();
        ForwardStack = nav.ForwardStack.ToNotifyCollectionChanged();
        HotKey = cmd.OnHotKey.ToReadOnlyBindableReactiveProperty();
    }

    public NotifyCollectionChangedSynchronizedViewList<NavigationPath> ForwardStack { get; }

    public NotifyCollectionChangedSynchronizedViewList<NavigationPath> BackwardStack { get; }

    public NotifyCollectionChangedSynchronizedViewList<DebugPageViewModel> Pages { get; }
    public IReadOnlyBindableReactiveProperty<NavigationPath> SelectedControlPath { get; }

    public IReadOnlyBindableReactiveProperty<HotKeyInfo?> HotKey { get; }

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

    public NotifyCollectionChangedSynchronizedViewList<CommandSnapshot> RedoStack { get; }

    public NotifyCollectionChangedSynchronizedViewList<CommandSnapshot> UndoStack { get; }

    protected override void Dispose(bool disposing)
    {
        // TODO: Implement Dispose
    }
}
