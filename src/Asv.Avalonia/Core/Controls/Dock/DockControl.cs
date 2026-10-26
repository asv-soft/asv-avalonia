using System.Collections.Specialized;
using Asv.Common;
using Asv.Modeling;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Animation;
using Avalonia.Media.Imaging;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Threading;
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
    private const double DragThreshold = 8;
    private DockTabItem? _dragTab;
    private DockTabStripItem? _dragHeader;
    private IPointer? _dragPointer;
    private Window? _dragOwner;
    private DockDragPreview? _dragPreview;
    private Point _dragStart;
    private Point _headerOffset;
    private double _headerOpacity;
    private bool _dragStarted;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ResetDrag(immediately: true);
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

                if (ReferenceEquals(_dragTab, dockTabItem))
                {
                    ResetDrag(immediately: true);
                }
                if (dockTabItem.Header is DockTabStripItem header)
                {
                    header.PointerPressed -= PressedHandler;
                    header.PointerMoved -= PointerMovedHandler;
                    header.PointerReleased -= PointerReleasedHandler;
                    header.PointerCaptureLost -= PointerCaptureLostHandler;
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

        if (change.Property == SelectedItemProperty && _mainTabControl != null)
        {
            var selected = _mainTabControl
                .Items.OfType<DockTabItem>()
                .FirstOrDefault(_ => _.Content == change.NewValue);
            if (selected is null)
            {
                if (change.NewValue is IPage page)
                {
                    page.Rise(new PageActivationRequestedEvent(page)).SafeFireAndForget();
                }
                EnsureDockTabItemFromCfgSelected();
                return;
            }

            SelectTab(selected);
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
            SelectTab(tab, notifyLayoutChanged: false);
        }
    }

    private DockTabItem CreateDockTabItem(IPage content)
    {
        var header = new DockTabStripItem { Content = content, Background = Brushes.Transparent };

        header.PointerPressed -= PressedHandler;
        header.PointerMoved -= PointerMovedHandler;
        header.PointerReleased -= PointerReleasedHandler;
        header.PointerCaptureLost -= PointerCaptureLostHandler;
        header.PointerPressed += PressedHandler;
        header.PointerMoved += PointerMovedHandler;
        header.PointerReleased += PointerReleasedHandler;
        header.PointerCaptureLost += PointerCaptureLostHandler;

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
        if (sender is not DockTabStripItem header || e.Handled
            || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed
            || TopLevel.GetTopLevel(this) is not Window window)
        {
            return;
        }
        if (e.Source is Visual source && (source is Button || source.FindAncestorOfType<Button>() != null))
        {
            return;
        }
        var tab = _mainTabControl.Items.OfType<DockTabItem>().FirstOrDefault(item => ReferenceEquals(item.Header, header));
        if (tab?.Content is not { } page)
        {
            return;
        }
        ResetDrag(immediately: true);
        if (SelectedItem is not IPage current || current.Id != page.Id)
        {
            _config.SelectedDockTabItemId = tab.Id;
            NotifyLayoutChanged();
            page.GoTo(page.GetPathFromRoot()).SafeFireAndForget();
        }
        _selectedTab = tab;
        _dragTab = tab;
        _dragHeader = header;
        _dragOwner = window;
        _dragPointer = e.Pointer;
        _dragStart = e.GetPosition(window);
        _headerOffset = e.GetPosition(header);
        _headerOpacity = header.Opacity;
        window.AddHandler(KeyDownEvent, DragKeyDown, RoutingStrategies.Tunnel, handledEventsToo: true);
        window.Deactivated += DragOwnerDeactivated;
        window.Closed += DragOwnerClosed;
        e.Pointer.Capture(header);
        e.Handled = true;
    }

    private void PointerMovedHandler(object? sender, PointerEventArgs e)
    {
        if (_dragTab?.Content is not { } page || _dragOwner is not { } owner
            || !ReferenceEquals(e.Pointer, _dragPointer))
        {
            return;
        }
        if (!e.GetCurrentPoint(owner).Properties.IsLeftButtonPressed)
        {
            ResetDrag(immediately: true);
            return;
        }
        var position = e.GetPosition(owner);
        if (!_dragStarted)
        {
            var deltaX = position.X - _dragStart.X;
            var deltaY = position.Y - _dragStart.Y;
            if ((deltaX * deltaX) + (deltaY * deltaY) < DragThreshold * DragThreshold)
            {
                return;
            }
            _dragStarted = true;
            _dragHeader?.SetCurrentValue(OpacityProperty, 0.4);
            _dragPreview = new DockDragPreview(page, CreateDragSnapshot(owner.RenderScaling))
            {
                RequestedThemeVariant = owner.ActualThemeVariant,
            };
            _dragPreview.MoveTo(owner.PointToScreen(position), owner.RenderScaling, !new Rect(owner.ClientSize).Contains(position));
            _dragPreview.Show(owner);
        }
        _dragPreview?.MoveTo(owner.PointToScreen(position), owner.RenderScaling, !new Rect(owner.ClientSize).Contains(position));
        e.Handled = true;
    }

    private void PointerReleasedHandler(object? sender, PointerReleasedEventArgs e)
    {
        if (!ReferenceEquals(e.Pointer, _dragPointer) || e.InitialPressMouseButton != MouseButton.Left)
        {
            return;
        }
        var tab = _dragTab;
        PixelPoint? dropPosition = null;
        if (_dragStarted && _dragOwner is { } owner && !new Rect(owner.ClientSize).Contains(e.GetPosition(owner)))
        {
            var pointer = owner.PointToScreen(e.GetPosition(owner));
            dropPosition = new PixelPoint(
                pointer.X - (int)(_headerOffset.X * owner.RenderScaling),
                pointer.Y - (int)(_headerOffset.Y * owner.RenderScaling));
            if (owner.Screens.ScreenFromPoint(pointer) is { } screen)
            {
                var area = screen.WorkingArea;
                dropPosition = new PixelPoint(
                    Math.Clamp(dropPosition.Value.X, area.X, Math.Max(area.X, area.Right - 160)),
                    Math.Clamp(dropPosition.Value.Y, area.Y, Math.Max(area.Y, area.Bottom - 40)));
            }
        }
        ResetDrag(immediately: false);
        if (tab != null && dropPosition != null && _mainTabControl.Items.Contains(tab))
        {
            DetachTab(tab, dropPosition);
        }
        e.Handled = true;
    }

    private RenderTargetBitmap? CreateDragSnapshot(double scaling)
    {
        if (!ReferenceEquals(_mainTabControl.SelectedItem, _dragTab))
        {
            return null;
        }
        var content = _mainTabControl.GetVisualDescendants().OfType<ContentPresenter>()
            .FirstOrDefault(presenter => presenter.Name == "PART_SelectedContentHost");
        if (content == null || content.Bounds.Width <= 0 || content.Bounds.Height <= 0)
        {
            return null;
        }
        RenderTargetBitmap? bitmap = null;
        try
        {
            bitmap = new RenderTargetBitmap(new PixelSize((int)(256 * scaling), (int)(136 * scaling)), new Vector(96 * scaling, 96 * scaling));
            using var context = bitmap.CreateDrawingContext();
            context.DrawRectangle(new VisualBrush(content) { Stretch = Stretch.Uniform }, null, new Rect(0, 0, 256, 136));
            return bitmap;
        }
        catch (Exception error) when (error is NotSupportedException or InvalidOperationException or ArgumentException)
        {
            bitmap?.Dispose();
            return null;
        }
    }

    private void DragKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && _dragTab != null)
        {
            ResetDrag(immediately: false);
            e.Handled = true;
        }
    }

    private void PointerCaptureLostHandler(object? sender, PointerCaptureLostEventArgs e) => ResetDrag(immediately: true);
    private void DragOwnerDeactivated(object? sender, EventArgs e) => ResetDrag(immediately: true);
    private void DragOwnerClosed(object? sender, EventArgs e) => ResetDrag(immediately: true);

    private void ResetDrag(bool immediately)
    {
        var pointer = _dragPointer;
        var header = _dragHeader;
        var preview = _dragPreview;
        if (_dragOwner is { } owner)
        {
            owner.RemoveHandler(KeyDownEvent, DragKeyDown);
            owner.Deactivated -= DragOwnerDeactivated;
            owner.Closed -= DragOwnerClosed;
        }
        _dragPointer = null;
        _dragTab = null;
        _dragHeader = null;
        _dragOwner = null;
        _dragPreview = null;
        _dragStarted = false;
        if (header != null)
        {
            header.SetCurrentValue(OpacityProperty, _headerOpacity);
        }
        if (pointer != null && ReferenceEquals(pointer.Captured, header))
        {
            pointer.Capture(null);
        }
        preview?.ClosePreview(immediately);
    }

    private void DetachTab(DockTabItem tab, PixelPoint? position = null)
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
            UpdateLayout();
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

        if (position is { } location)
        {
            win.Position = location;
            win.Width = Math.Max(480, Bounds.Width);
            win.Height = Math.Max(320, Bounds.Height);
            win.Opacity = 0;
            win.Transitions = new Transitions
            {
                new DoubleTransition { Property = OpacityProperty, Duration = TimeSpan.FromMilliseconds(120) },
            };
        }
        var isShutdown = false;
        var activation = page.Events.Catch<PageActivationRequestedEvent>(_ =>
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                ActivateWindow();
            }
            else
            {
                Dispatcher.UIThread.Post(ActivateWindow);
            }
        });

        win.Closing += OnWindowClosing;
        win.Closed += OnWindowClosed;
        win.Show();
        win.Opacity = 1;
        return;

        void ActivateWindow()
        {
            if (!win.IsVisible)
            {
                return;
            }
            if (win.WindowState == WindowState.Minimized)
            {
                win.WindowState = WindowState.Normal;
            }
            win.Activate();
            win.Focus();
        }

        void OnWindowClosing(object? source, WindowClosingEventArgs args)
        {
            isShutdown = args.CloseReason == WindowCloseReason.ApplicationShutdown;
        }

        void OnWindowClosed(object? source, EventArgs args)
        {
            activation.Dispose();
            win.Closing -= OnWindowClosing;
            win.Closed -= OnWindowClosed;

            if (isShutdown)
            {
                return;
            }

            // Release the old view before another presenter binds to the same page.
            win.Page = null!;
            win.Content = null;
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

        SetCurrentValue(SelectedItemProperty, null);
        SetCurrentValue(SelectedItemProperty, page);
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

        SelectTab(fromCfg, notifyLayoutChanged: false);
    }

    private void SelectTab(DockTabItem tab, bool notifyLayoutChanged = true)
    {
        foreach (var item in _mainTabControl.Items.OfType<DockTabItem>().Where(it => it.IsSelected))
        {
            item.IsSelected = false;
        }

        _mainTabControl.SelectedItem = tab;
        tab.IsSelected = true;
        _selectedTab = tab;
        _config.SelectedDockTabItemId = tab.Id;

        if (notifyLayoutChanged)
        {
            NotifyLayoutChanged();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ResetDrag(immediately: true);
        _layout?.Dispose();
        _layout = null;

        if (Items is INotifyCollectionChanged notifyCol)
        {
            notifyCol.CollectionChanged -= ItemsCollectionChanged;
        }

        foreach (var item in _mainTabControl?.Items.OfType<DockTabItem>() ?? [])
        {
            if (item.Header is not DockTabStripItem header)
            {
                continue;
            }

            header.PointerPressed -= PressedHandler;
            header.PointerMoved -= PointerMovedHandler;
            header.PointerReleased -= PointerReleasedHandler;
            header.PointerCaptureLost -= PointerCaptureLostHandler;
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
            header.PointerCaptureLost -= PointerCaptureLostHandler;
        }

        base.OnDetachedFromVisualTree(e);
    }

    private void RegisterLayout()
    {
        _layout?.Dispose();
        _layout = this.RegisterLayout(nameof(DockControl), LoadLayout, SaveLayout, _layoutChanged);
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
