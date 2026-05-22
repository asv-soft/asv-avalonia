using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.VisualTree;
using R3;

namespace Asv.Avalonia;

internal enum DockItemState
{
    Tab,
    Window,
}

internal sealed class DockControlConfig
{
    public Dictionary<string, DockItemState> DockItemStates { get; set; } = new();
    public string? SelectedDockTabItemId { get; set; }
}

public class DockControl : SelectingItemsControl, ICustomHitTest
{
    private const string PART_MainTabControl = "PART_MainTabControl";
    private readonly Subject<Unit> _layoutChanged = new();
    private readonly List<DockTabItem> _windowedItems = [];

    private DockTabItem? _selectedTab;
    private DockTabControl _mainTabControl = null!;
    private DockControlConfig _config = new();
    private IDisposable? _layout;
    private bool _internalLayoutChange;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _mainTabControl =
            e.NameScope.Find<DockTabControl>(PART_MainTabControl)
            ?? throw new ApplicationException(
                $"{PART_MainTabControl} not found in {nameof(DockControl)} template."
            );
        LogicalChildren.Add(_mainTabControl);

        RegisterLayout();

        if (Items is INotifyCollectionChanged notifyCol)
        {
            notifyCol.CollectionChanged -= ItemsCollectionChanged;
            notifyCol.CollectionChanged += ItemsCollectionChanged;
        }

        foreach (var content in Items)
        {
            AddTabIfNotExists(content);
        }
    }

    private void ItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e is { Action: NotifyCollectionChangedAction.Add, NewItems: not null })
        {
            foreach (var ni in e.NewItems)
            {
                AddTabIfNotExists(ni);
            }

            return;
        }

        if (e is { Action: NotifyCollectionChangedAction.Remove, OldItems: not null })
        {
            foreach (var removedItem in e.OldItems)
            {
                var dockTabItem = _mainTabControl
                    ?.Items.OfType<DockTabItem>()
                    .FirstOrDefault(item => item.Content == removedItem);

                if (dockTabItem is null)
                {
                    var windowedItem = _windowedItems.FirstOrDefault(item =>
                        item.Content == removedItem
                    );
                    if (windowedItem is not null)
                    {
                        _windowedItems.Remove(windowedItem);
                        NotifyLayoutChanged();
                    }

                    continue;
                }

                if (dockTabItem.Header is DockTabStripItem header)
                {
                    header.PointerPressed -= PressedHandler;
                    header.PointerMoved -= PointerMovedHandler;
                    header.PointerReleased -= PointerReleasedHandler;
                }

                _mainTabControl?.Items.Remove(dockTabItem);
                NotifyLayoutChanged();
            }

            return;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedItemProperty)
        {
            var selected = _mainTabControl
                .Items.OfType<DockTabItem>()
                .FirstOrDefault(_ => _.Content == change.NewValue);
            if (selected is null)
            {
                EnsureDockTabItemFromCfgSelected();
                return;
            }

            foreach (
                var item in _mainTabControl.Items.OfType<DockTabItem>().Where(it => it.IsSelected)
            )
            {
                item.IsSelected = false;
            }

            _mainTabControl.SelectedItem = selected;
            _selectedTab = selected;
            _config.SelectedDockTabItemId = selected.Id;
            NotifyLayoutChanged();
        }
    }

    private void AddTabIfNotExists(object? content)
    {
        if (content is not IPage page)
        {
            return;
        }

        if (_mainTabControl.Items.OfType<DockTabItem>().Any(it => it.Id == page.Id.ToString()))
        {
            return;
        }

        if (_windowedItems.Any(it => it.Id == page.Id.ToString()))
        {
            return;
        }

        var tab = CreateDockTabItem(page);

        if (_config.DockItemStates.TryGetValue(page.Id.ToString(), out var state))
        {
            if (state == DockItemState.Window)
            {
                DetachTab(tab);
                return;
            }
        }
        else
        {
            _config.DockItemStates[page.Id.ToString()] = DockItemState.Tab;
        }

        _mainTabControl.Items.Add(tab);

        if (SelectedItem is IPage selectedPage && selectedPage.Id == page.Id)
        {
            _mainTabControl.SelectedItem = tab;
            _selectedTab = tab;
        }
    }

    private DockTabItem CreateDockTabItem(IPage content)
    {
        var header = new DockTabStripItem { Content = content, Background = Brushes.Transparent };

        header.PointerPressed -= PressedHandler;
        header.PointerMoved -= PointerMovedHandler;
        header.PointerReleased -= PointerReleasedHandler;
        header.PointerPressed += PressedHandler;
        header.PointerMoved += PointerMovedHandler;
        header.PointerReleased += PointerReleasedHandler;

        var tab = new DockTabItem
        {
            Id = content.Id.ToString(),
            Content = content,
            Header = header,
        };
        return tab;
    }

    private void PressedHandler(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not DockTabStripItem tabStrip)
        {
            return;
        }

        if (e.Handled)
        {
            return;
        }

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var source = e.Source as Visual;
        var tab = source as DockTabItem ?? source?.FindAncestorOfType<DockTabItem>();
        if (tab is not null)
        {
            if (SelectedItem is IPage current && current.Id != tab.Content?.Id)
            {
                _config.SelectedDockTabItemId = tab.Id;
                NotifyLayoutChanged();
            }

            _selectedTab = tab;
            SelectedItem = tab.Content;
        }

        e.Pointer.Capture(tabStrip);
        e.Handled = true;
    }

    private void PointerMovedHandler(object? sender, PointerEventArgs e)
    {
        // TODO add visual drag indicator
    }

    private void PointerReleasedHandler(object? sender, PointerReleasedEventArgs e)
    {
        if (_selectedTab is null)
        {
            e.Pointer.Capture(null);
            return;
        }

        if (TopLevel.GetTopLevel(this) is not Window window)
        {
            return;
        }

        var cursorPosition = e.GetPosition(window);

        if (!Bounds.Contains(cursorPosition))
        {
            DetachTab(_selectedTab);
        }

        e.Pointer.Capture(null);
    }

    private void DetachTab(DockTabItem tab)
    {
        if (tab.Content is not { } page)
        {
            return;
        }

        if (_windowedItems.Any(it => it.Id == tab.Id))
        {
            return;
        }

        var dockTabItem = _mainTabControl
            .Items.OfType<DockTabItem>()
            .FirstOrDefault(item => item.Id == tab.Id);

        if (dockTabItem is not null)
        {
            _mainTabControl.Items.Remove(dockTabItem);
            _selectedTab = _mainTabControl.SelectedItem as DockTabItem;
            _config.SelectedDockTabItemId = _selectedTab?.Id;
        }

        var win = new DockWindow
        {
            Id = page.Id.ToString(),
            Page = page,
            DataContext = tab.DataContext,
        };

        _windowedItems.Add(tab);
        _config.DockItemStates[page.Id.ToString()] = DockItemState.Window;
        NotifyLayoutChanged();

        win.Closing += OnWindowClosing;
        win.Show();
        return;

        void OnWindowClosing(object? source, WindowClosingEventArgs args)
        {
            win.Closing -= OnWindowClosing;

            if (args.CloseReason == WindowCloseReason.ApplicationShutdown)
            {
                return;
            }

            AttachTab(tab);
        }
    }

    private void AttachTab(DockTabItem tab)
    {
        if (!_mainTabControl.Items.Contains(tab))
        {
            _mainTabControl.Items.Add(tab);
        }

        _windowedItems.Remove(tab);

        if (tab.Content is not { } page)
        {
            return;
        }

        _config.DockItemStates[page.Id.ToString()] = DockItemState.Tab;
        _selectedTab = tab;
        _config.SelectedDockTabItemId = tab.Id;

        SelectedItem = null;
        SelectedItem = page;
        NotifyLayoutChanged();
    }

    private void EnsureDockTabItemFromCfgSelected()
    {
        if (_config.SelectedDockTabItemId is null)
        {
            return;
        }

        var fromCfg = _mainTabControl
            .Items.OfType<DockTabItem>()
            .FirstOrDefault(it => it.Id == _config.SelectedDockTabItemId);

        if (fromCfg is null)
        {
            return;
        }

        foreach (var item in _mainTabControl.Items.OfType<DockTabItem>().Where(it => it.IsSelected))
        {
            item.IsSelected = false;
        }

        _mainTabControl.SelectedItem = fromCfg;
        _selectedTab = fromCfg;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _layout?.Dispose();
        _layout = null;

        if (Items is INotifyCollectionChanged notifyCol)
        {
            notifyCol.CollectionChanged -= ItemsCollectionChanged;
        }

        foreach (var item in _mainTabControl.Items.OfType<DockTabItem>())
        {
            if (item.Header is not DockTabStripItem header)
            {
                continue;
            }

            header.PointerPressed -= PressedHandler;
            header.PointerMoved -= PointerMovedHandler;
            header.PointerReleased -= PointerReleasedHandler;
        }

        foreach (var item in _windowedItems)
        {
            if (item.Header is not DockTabStripItem header)
            {
                continue;
            }

            header.PointerPressed -= PressedHandler;
            header.PointerMoved -= PointerMovedHandler;
            header.PointerReleased -= PointerReleasedHandler;
        }

        base.OnDetachedFromVisualTree(e);
    }

    private void RegisterLayout()
    {
        _layout?.Dispose();
        _layout = this.RegisterLayout<DockControlConfig, Unit>(
            nameof(DockControl),
            LoadLayout,
            SaveLayout,
            _layoutChanged
        );
    }

    private void LoadLayout(DockControlConfig config)
    {
        _internalLayoutChange = true;
        try
        {
            _config = config;
            _config.DockItemStates ??= new Dictionary<string, DockItemState>();
            ApplyLayout();
        }
        finally
        {
            _internalLayoutChange = false;
        }
    }

    private DockControlConfig? SaveLayout()
    {
        if (_internalLayoutChange)
        {
            return null;
        }

        var config = new DockControlConfig
        {
            SelectedDockTabItemId =
                (_mainTabControl.SelectedItem as DockTabItem)?.Id ?? _selectedTab?.Id,
        };

        foreach (var item in _mainTabControl.Items.OfType<DockTabItem>())
        {
            if (item.Content is { } page)
            {
                config.DockItemStates[page.Id.ToString()] = DockItemState.Tab;
            }
        }

        foreach (var item in _windowedItems)
        {
            if (item.Content is { } page)
            {
                config.DockItemStates[page.Id.ToString()] = DockItemState.Window;
            }
        }

        _config = config;
        return config;
    }

    private void ApplyLayout()
    {
        foreach (var content in Items)
        {
            AddTabIfNotExists(content);
        }

        foreach (var tab in _mainTabControl.Items.OfType<DockTabItem>().ToArray())
        {
            if (
                _config.DockItemStates.TryGetValue(tab.Id, out var state)
                && state == DockItemState.Window
            )
            {
                DetachTab(tab);
            }
        }

        EnsureDockTabItemFromCfgSelected();
    }

    private void NotifyLayoutChanged()
    {
        if (_internalLayoutChange)
        {
            return;
        }

        _layoutChanged.OnNext(Unit.Default);
    }

    public static readonly DirectProperty<DockControl, double> LeftDisabledHitTestHeightProperty =
        AvaloniaProperty.RegisterDirect<DockControl, double>(
            nameof(DisabledHitTestHeight),
            o => o.DisabledHitTestHeight,
            (o, v) => o.DisabledHitTestHeight = v
        );

    public double DisabledHitTestHeight
    {
        get;
        set => SetAndRaise(LeftDisabledHitTestHeightProperty, ref field, value);
    }

    public bool HitTest(Point point)
    {
        var leftBounds = new Rect(0, 0, Padding.Left, DisabledHitTestHeight);
        if (leftBounds.Contains(point))
        {
            return false;
        }

        var rightBounds = new Rect(
            this.Bounds.Width - Padding.Right,
            0,
            Padding.Right,
            DisabledHitTestHeight
        );
        if (rightBounds.Contains(point))
        {
            return false;
        }

        return true;
    }
}
