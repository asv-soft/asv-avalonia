﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Asv.Avalonia;

public partial class WorkspacePanel
{
    public static readonly AttachedProperty<WorkspaceDock> DockProperty =
        AvaloniaProperty.RegisterAttached<WorkspacePanel, Control, WorkspaceDock>("Dock");

    public static void SetDock(Control obj, WorkspaceDock value) =>
        obj.SetValue(DockProperty, value);

    public static WorkspaceDock GetDock(Control obj) => obj.GetValue(DockProperty);

    public static readonly StyledProperty<GridLength> LeftWidthProperty = AvaloniaProperty.Register<
        WorkspacePanel,
        GridLength
    >(nameof(LeftWidth), new GridLength(1, GridUnitType.Star));

    public GridLength LeftWidth
    {
        get => GetValue(LeftWidthProperty);
        set => SetValue(LeftWidthProperty, value);
    }

    public static readonly StyledProperty<GridLength> CentralWidthProperty =
        AvaloniaProperty.Register<WorkspacePanel, GridLength>(
            nameof(CentralWidth),
            new GridLength(3, GridUnitType.Star)
        );
    public GridLength CentralWidth
    {
        get => GetValue(CentralWidthProperty);
        set => SetValue(CentralWidthProperty, value);
    }

    public static readonly StyledProperty<GridLength> RightWidthProperty =
        AvaloniaProperty.Register<WorkspacePanel, GridLength>(
            nameof(RightWidth),
            new GridLength(1, GridUnitType.Star)
        );
    public GridLength RightWidth
    {
        get => GetValue(RightWidthProperty);
        set => SetValue(RightWidthProperty, value);
    }

    public static readonly StyledProperty<GridLength> BottomHeightProperty =
        AvaloniaProperty.Register<WorkspacePanel, GridLength>(
            nameof(BottomHeight),
            new GridLength(1, GridUnitType.Star)
        );
    public GridLength BottomHeight
    {
        get => GetValue(BottomHeightProperty);
        set => SetValue(BottomHeightProperty, value);
    }

    public static readonly StyledProperty<GridLength> CentralHeightProperty =
        AvaloniaProperty.Register<WorkspacePanel, GridLength>(
            nameof(CentralHeight),
            new GridLength(3, GridUnitType.Star)
        );

    public GridLength CentralHeight
    {
        get => GetValue(CentralHeightProperty);
        set => SetValue(CentralHeightProperty, value);
    }

    public static readonly StyledProperty<double> MinLeftWidthProperty = AvaloniaProperty.Register<
        WorkspacePanel,
        double
    >(nameof(MinLeftWidth));

    public double MinLeftWidth
    {
        get => GetValue(MinLeftWidthProperty);
        set => SetValue(MinLeftWidthProperty, value);
    }
    public static readonly StyledProperty<double> MinRightWidthProperty = AvaloniaProperty.Register<
        WorkspacePanel,
        double
    >(nameof(MinRightWidth));

    public double MinRightWidth
    {
        get => GetValue(MinRightWidthProperty);
        set => SetValue(MinRightWidthProperty, value);
    }

    public static readonly StyledProperty<double> MinBottomHeightProperty =
        AvaloniaProperty.Register<WorkspacePanel, double>(nameof(MinBottomHeight));

    public double MinBottomHeight
    {
        get => GetValue(MinBottomHeightProperty);
        set => SetValue(MinBottomHeightProperty, value);
    }

    public static readonly StyledProperty<double> MinCentralHeightProperty =
        AvaloniaProperty.Register<WorkspacePanel, double>(nameof(MinCentralHeight));
    public double MinCentralHeight
    {
        get => GetValue(MinCentralHeightProperty);
        set => SetValue(MinCentralHeightProperty, value);
    }

    public static readonly StyledProperty<double> MinCentralWidthProperty =
        AvaloniaProperty.Register<WorkspacePanel, double>(nameof(MinCentralWidth));
    public double MinCentralWidth
    {
        get => GetValue(MinCentralWidthProperty);
        set => SetValue(MinCentralWidthProperty, value);
    }

    public static readonly RoutedEvent<WorkspaceEventArgs> WorkspaceChangedEvent =
        RoutedEvent.Register<WorkspacePanel, WorkspaceEventArgs>(
            nameof(WorkspaceChanged),
            RoutingStrategies.Bubble
        );

    public event EventHandler<WorkspaceEventArgs>? WorkspaceChanged
    {
        add => AddHandler(WorkspaceChangedEvent, value);
        remove => RemoveHandler(WorkspaceChangedEvent, value);
    }
}

public class WorkspaceEventArgs : RoutedEventArgs
{
    public double LeftColumnActualWidth { get; set; }
    public double CenterColumnActualWidth { get; set; }
    public double RightColumnActualWidth { get; set; }
    public double CenterRowActualHeight { get; set; }
    public double BottomRowActualHeight { get; set; }
}
