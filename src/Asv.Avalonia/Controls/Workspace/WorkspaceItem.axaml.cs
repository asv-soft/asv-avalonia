﻿using Avalonia;
using Avalonia.Controls.Primitives;

namespace Asv.Avalonia;

public class WorkspaceItem : HeaderedContentControl
{
    public WorkspaceItem() { }

    public static readonly StyledProperty<WorkspaceDock> PositionProperty =
        AvaloniaProperty.Register<WorkspaceItem, WorkspaceDock>(nameof(Position));

    public WorkspaceDock Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }
}
