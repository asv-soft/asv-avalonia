using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace Asv.Avalonia.Map;

public enum WorkspaceDock
{
    Left,
    Right,
    Bottom,
}

public class Workspace : ItemsControl
{
    private StackPanel? _leftPanel;
    private StackPanel? _rightPanel;
    private AvaloniaList<WorkspaceItem>? _bottomPanel;

    static Workspace() { }

    public Workspace()
    {
        ItemsView.CollectionChanged += ItemsChanged;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _leftPanel = e.NameScope.Find<StackPanel>("PART_LeftPanel");
        _rightPanel = e.NameScope.Find<StackPanel>("PART_RightPanel");
        var tab = e.NameScope.Find<TabControl>("PART_BottomTab");
        if (tab != null)
        {
            tab.ItemsSource = _bottomPanel = new AvaloniaList<WorkspaceItem>();
        }

        RecalculatePosition();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        ItemsView.CollectionChanged -= ItemsChanged;
    }

    private void ItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e is { Action: NotifyCollectionChangedAction.Remove, OldItems: not null })
        {
            foreach (var item in e.OldItems)
            {
                if (item == null)
                    continue;
                var container = item as WorkspaceItem ?? ContainerFromItem(item) as WorkspaceItem;
                if (container == null)
                    continue;
                _leftPanel?.Children.Remove(container);
                _rightPanel?.Children.Remove(container);
                _bottomPanel?.Remove(container);
            }
        }
        RecalculatePosition();
    }

    private void RecalculatePosition()
    {
        if (_leftPanel == null || _rightPanel == null || _bottomPanel == null)
            return;
        foreach (var item in ItemsView)
        {
            if (item == null)
                continue;
            var container = item as WorkspaceItem ?? ContainerFromItem(item) as WorkspaceItem;
            if (container == null)
                continue;
            switch (container.Position)
            {
                case WorkspaceDock.Left:
                    if (_leftPanel.Children.Contains(container))
                        continue;
                    _rightPanel.Children.Remove(container);
                    _bottomPanel.Remove(container);
                    _leftPanel.Children.Add(container);
                    break;
                case WorkspaceDock.Right:
                    if (_rightPanel.Children.Contains(container))
                        continue;
                    _leftPanel.Children.Remove(container);
                    _bottomPanel.Remove(container);
                    _rightPanel.Children.Add(container);
                    break;
                case WorkspaceDock.Bottom:
                    if (_bottomPanel.Contains(container))
                        continue;
                    _leftPanel.Children.Remove(container);
                    _rightPanel.Children.Remove(container);
                    _bottomPanel.Add(container);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    protected override Control CreateContainerForItemOverride(
        object? item,
        int index,
        object? recycleKey
    )
    {
        return new WorkspaceItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<WorkspaceItem>(item, out recycleKey);
    }
}
