using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Asv.Avalonia;

public enum WorkspaceDock
{
    Left,
    Right,
    Bottom,
    Center,
}

public class Workspace : ItemsControl
{
    public static readonly StyledProperty<string> LayoutIdProperty = AvaloniaProperty.Register<
        Workspace,
        string
    >(nameof(LayoutId), nameof(WorkspacePanel));

    static Workspace() { }

    public Workspace() { }

    public string LayoutId
    {
        get => GetValue(LayoutIdProperty);
        set => SetValue(LayoutIdProperty, value);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
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
