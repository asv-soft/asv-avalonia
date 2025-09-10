using System.Collections.Specialized;
using Asv.Cfg;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Asv.Avalonia;

internal enum DockItemState
{
    Tab,
    Window,
}

internal sealed class DockControlConfig
{
    public readonly Dictionary<string, DockItemState> DockItemStates = new();
}

public partial class DockControl : SelectingItemsControl
{
    private const string PART_MainTabControl = "PART_MainTabControl";
    private readonly List<DockTabItem> _windowedItems = [];

    private DockTabItem? _selectedTab;
    private DockTabControl _mainTabControl = null!;
    private DockControlConfig _config = null!;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _mainTabControl =
            e.NameScope.Find<DockTabControl>(PART_MainTabControl)
            ?? throw new ApplicationException(
                $"{PART_MainTabControl} not found in {nameof(DockControl)} template."
            );

        ArgumentNullException.ThrowIfNull(Configuration);
        _config = Configuration.Get<DockControlConfig>();

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
                    return;
                }

                if (dockTabItem.Header is DockTabStripItem header)
                {
                    header.PointerPressed -= PressedHandler;
                    header.PointerMoved -= PointerMovedHandler;
                }

                _mainTabControl?.Items.Remove(dockTabItem);
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
                return;
            }

            foreach (var item in _mainTabControl.Items.OfType<DockTabItem>())
            {
                item.IsSelected = false;
            }

            selected.IsSelected = true;
            SelectedItem = selected.Content;
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
            Configuration?.Set(_config);
        }

        _mainTabControl.Items.Add(tab);
    }

    private DockTabItem CreateDockTabItem(IPage content)
    {
        var header = new DockTabStripItem { Content = content };

        header.PointerPressed -= PressedHandler;
        header.PointerMoved -= PointerMovedHandler;
        header.PointerPressed += PressedHandler;
        header.PointerMoved += PointerMovedHandler;

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
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var source = e.Source as Visual;
        var tab = source as DockTabItem ?? source?.FindAncestorOfType<DockTabItem>();
        if (tab is not null)
        {
            _selectedTab = tab;
            SelectedItem = tab.Content;
        }

        e.Pointer.Capture(this);
    }

    private void PointerMovedHandler(object? sender, PointerEventArgs e)
    {
        // TODO add visual drag indicator
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_selectedTab is null)
        {
            return;
        }

        if (this.GetVisualRoot() is not Window window)
        {
            return;
        }

        var cursorPosition = e.GetPosition(window);

        if (!Bounds.Contains(cursorPosition))
        {
            DetachTab(_selectedTab);
        }

        _selectedTab = null;
    }

    private void DetachTab(DockTabItem tab)
    {
        if (tab.Content is null)
        {
            return;
        }

        var dockTabItem = _mainTabControl
            .Items.OfType<DockTabItem>()
            .FirstOrDefault(item => item.Id == tab.Id);

        if (dockTabItem is not null)
        {
            _mainTabControl.Items.Remove(dockTabItem);
        }

        var win = new DockWindow
        {
            Id = tab.Content.Id.ToString(),
            Content = tab.Content,
            Title = tab.Content.Title,
        };

        _windowedItems.Add(tab);
        _config.DockItemStates[tab.Content.Id.ToString()] = DockItemState.Window;
        Configuration?.Set(_config);

        win.Closing += AttachTab;
        win.Show();
        return;

        void AttachTab(object? source, WindowClosingEventArgs args)
        {
            win.Closing -= AttachTab;

            if (args.CloseReason == WindowCloseReason.ApplicationShutdown)
            {
                return;
            }

            if (!_mainTabControl.Items.Contains(tab))
            {
                _mainTabControl.Items.Add(tab);
            }

            _windowedItems.Remove(tab);

            if (tab.Content is null)
            {
                return;
            }

            _config.DockItemStates[tab.Content.Id.ToString()] = DockItemState.Tab;
            Configuration?.Set(_config);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
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
        }

        foreach (var item in _windowedItems)
        {
            if (item.Header is not DockTabStripItem header)
            {
                continue;
            }

            header.PointerPressed -= PressedHandler;
            header.PointerMoved -= PointerMovedHandler;
        }

        base.OnDetachedFromVisualTree(e);
    }
}
