using System;
using Avalonia;
using Avalonia.Controls;

namespace Asv.Avalonia.Example;

public class ExampleBlocksGrid : Grid
{
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ValidateChildren();
    }

    private void ValidateChildren()
    {
        foreach (var child in Children)
        {
            if (child is not BaseExampleBlock)
            {
                throw new InvalidOperationException(
                    $"{nameof(ExampleBlocksGrid)} can only contain children of type {nameof(BaseExampleBlock)} or its descendants. "
                        + $"Found: {child.GetType().Name}"
                );
            }
        }
    }
}
