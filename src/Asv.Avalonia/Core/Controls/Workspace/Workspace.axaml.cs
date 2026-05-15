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
    static Workspace() { }

    public Workspace() { }

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
