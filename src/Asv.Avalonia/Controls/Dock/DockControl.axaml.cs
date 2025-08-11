using System.Collections.Specialized;
using System.ComponentModel;
using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using R3;

namespace Asv.Avalonia;

public class ShellItem
{
    public required string Id { get; init; }
    public TabItem TabControl { get; init; } = new();
}

public partial class DockControl : SelectingItemsControl // TODO: fix deletion
{
    private readonly List<ShellItem> _shellItems = [];
    private readonly List<ShellItem> _windowedItems = [];

    private TabItem? _selectedTab;
    private AdaptiveTabStripTabControl? _mainTabControl;

    public DockControl() { }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _mainTabControl =
            e.NameScope.Find<AdaptiveTabStripTabControl>("PART_MainTabControl")
            ?? throw new ApplicationException("PART_MainTabControl not found in DockControl.axaml");

        CreateOrUpdateTabs();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedItemProperty && _mainTabControl != null)
        {
            var selected = _shellItems.FirstOrDefault(_ => _.TabControl.Content == change.NewValue);
            if (selected != null)
            {
                foreach (var item in _shellItems)
                {
                    item.TabControl.IsSelected = false;
                }

                selected.TabControl.IsSelected = true;
                SelectedItem = selected.TabControl.Content;
            }
        }
        else if (change.Property == ItemCountProperty)
        {
            CreateOrUpdateTabs();
        }
    }

    protected override void LogicalChildrenCollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e
    )
    {
        base.LogicalChildrenCollectionChanged(sender, e);

        if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
        {
            foreach (var removedItem in e.OldItems)
            {
                var shellItem = _shellItems.FirstOrDefault(item =>
                    item.TabControl.Content == removedItem
                );
                if (shellItem != null)
                {
                    _shellItems.Remove(shellItem);
                    _mainTabControl?.Items.Remove(shellItem.TabControl);
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
        {
            foreach (var newItem in e.NewItems)
            {
                AddTabIfNotExists(newItem);
            }
        }
    }

    private void CreateOrUpdateTabs()
    {
        if (_mainTabControl == null)
        {
            return;
        }
        foreach (var content in Items)
        {
            AddTabIfNotExists(content);
        }
    }

    private void AddTabIfNotExists(object? content)
    {
        if (_mainTabControl == null)
        {
            return;
        }

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
        var shellItem = new ShellItem { Id = page.Id.ToString(), TabControl = tab };

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
        SubscribeToEvents(header);

        var tab = new TabItem { Content = content, Header = header };

        return tab;
    }

    private void SubscribeToEvents(TabStripItem header)
    {
        header.PointerPressed += PressedHandler;
        header.PointerMoved += PointerMovedHandler;
    }

    private void PressedHandler(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var source = e.Source as Visual;
        var tab = source as TabItem ?? source?.FindAncestorOfType<TabItem>();
        if (tab != null)
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

        if (_selectedTab == null)
        {
            return;
        }

        if (this.GetVisualRoot() is not Window window)
        {
            return;
        }

        var cursorPosition = e.GetPosition(window);

        if (!Bounds.Contains(e.GetPosition(this)))
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
        if (shellItem == null)
        {
            return;
        }

        _mainTabControl?.Items.Remove(tab);
        _shellItems.Remove(shellItem);

        var win = new DockWindow(shellItem.Id)
        {
            Content = tab.Content,
            Title = (tab.Header as TabStripItem)?.Content?.ToString(),
        };

        _windowedItems.Add(shellItem);
        win.Closing += (_, _) => AttachTab(shellItem);
        win.Show();
    }

    private void AttachTab(ShellItem shellItem)
    {
        if (_mainTabControl == null)
        {
            return;
        }

        if (!_shellItems.Contains(shellItem))
        {
            _shellItems.Add(shellItem);
        }

        if (!_mainTabControl.Items.Contains(shellItem.TabControl))
        {
            _mainTabControl.Items.Add(shellItem.TabControl);
        }

        _windowedItems.Remove(shellItem);
    }
}
