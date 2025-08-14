using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Asv.Avalonia;

public partial class DockControl : SelectingItemsControl
{
    private readonly List<DockItem> _shellItems = [];
    private readonly List<DockItem> _windowedItems = []; // TODO: remove try to remove windowedItems and shellItems

    private TabItem? _selectedTab;
    private DockTabControl _mainTabControl = null!;

    public DockControl() { }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _mainTabControl =
            e.NameScope.Find<DockTabControl>(PART_MainTabControl)
            ?? throw new ApplicationException(
                $"{PART_MainTabControl} not found in {nameof(DockControl)} template."
            );

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
                var shellItem = _shellItems.FirstOrDefault(item =>
                    item.TabControl.Content == removedItem
                );

                if (shellItem is not null)
                {
                    _shellItems.Remove(shellItem);
                    _mainTabControl?.Items.Remove(shellItem.TabControl);
                }
            }

            return;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedItemProperty)
        {
            var selected = _shellItems.FirstOrDefault(_ => _.TabControl.Content == change.NewValue);
            if (selected is null)
            {
                return;
            }

            foreach (var item in _shellItems)
            {
                item.TabControl.IsSelected = false;
            }

            selected.TabControl.IsSelected = true;
            SelectedItem = selected.TabControl.Content;
        }
    }

    private void AddTabIfNotExists(object? content)
    {
        if (content is not IPage page)
        {
            return;
        }

        if (_shellItems.Any(it => it.Id == page.Id.ToString()))
        {
            return;
        }

        if (_windowedItems.Any(it => it.Id == page.Id.ToString()))
        {
            return;
        }

        var tab = CreateTabItem(content);
        if (page.State == PageState.Window)
        {
            DetachTab(tab);
            return;
        }

        var shellItem = new DockItem { Id = page.Id.ToString(), TabControl = tab };

        _shellItems.Add(shellItem);
        _mainTabControl.Items.Add(tab);
    }

    private TabItem CreateTabItem(object content)
    {
        var header = new TabStripItem
        {
            Content = content,
            ContentTemplate = TabControlStripItemTemplate,
        };

        header.PointerPressed -= PressedHandler;
        header.PointerMoved -= PointerMovedHandler;
        header.PointerPressed += PressedHandler;
        header.PointerMoved += PointerMovedHandler;

        var tab = new TabItem { Content = content, Header = header };

        return tab;
    }

    private void PressedHandler(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var source = e.Source as Visual;
        var tab = source as TabItem ?? source?.FindAncestorOfType<TabItem>();
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

    private void DetachTab(TabItem tab)
    {
        if (tab.Content is not IPage page)
        {
            return;
        }

        var shellItem = _shellItems.FirstOrDefault(item => item.TabControl == tab);
        if (shellItem is null)
        {
            return;
        }

        _mainTabControl.Items.Remove(tab);
        _shellItems.Remove(shellItem);

        var win = new DockWindow(shellItem.Id) { Content = tab.Content, Title = page.Title };

        _windowedItems.Add(shellItem);
        page.State = PageState.Window;
        win.Closing += (_, _) => AttachTab(shellItem);
        win.Show();
    }

    private void AttachTab(DockItem dockItem)
    {
        if (!_shellItems.Contains(dockItem))
        {
            _shellItems.Add(dockItem);
        }

        if (!_mainTabControl.Items.Contains(dockItem.TabControl))
        {
            _mainTabControl.Items.Add(dockItem.TabControl);
        }

        _windowedItems.Remove(dockItem);

        if (dockItem.TabControl.Content is IPage page)
        {
            page.State = PageState.Tab;
        }
    }
}
