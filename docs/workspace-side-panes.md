# Workspace side panes

`Workspace` exposes independent `IsLeftPaneOpen` and `IsRightPaneOpen` flags for its left and right widget columns. Both default to `true` and support two-way binding. The workspace adds no pane headers, hide/show buttons, or header space; the page owns the controls and decides where to put them.

For example, put toggle buttons in a page toolbar and bind them to the workspace:

```xml
<Grid RowDefinitions="Auto,*">
    <StackPanel Orientation="Horizontal" Spacing="8">
        <ToggleButton
            Content="Filters"
            IsChecked="{Binding #PageWorkspace.IsLeftPaneOpen, Mode=TwoWay}" />
        <ToggleButton
            Content="Details"
            IsChecked="{Binding #PageWorkspace.IsRightPaneOpen, Mode=TwoWay}" />
    </StackPanel>
    <asv:Workspace
        x:Name="PageWorkspace"
        Grid.Row="1"
        ItemsSource="{Binding Widgets}" />
</Grid>
```

The same flags can be bound to view-model properties or changed programmatically. Place controls outside the side they hide so that users can reopen it. Empty sides consume no column space and retain their open/closed preference when widgets are moved away and back.

Hiding a side preserves each widget's instance, position, visibility and expansion state. The center receives the freed space. Reopening restores the last positive width; hiding one side keeps the other side's width stable.

Existing `RegisterWorkspaceLayout` registrations persist the two flags with the dimensions. Old snapshots without these flags open both sides. Widget expansion remains an independent setting.
