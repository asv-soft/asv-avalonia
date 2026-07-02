using Avalonia.Controls;

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
