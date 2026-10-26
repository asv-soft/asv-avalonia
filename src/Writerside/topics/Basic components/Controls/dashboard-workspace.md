# Dashboards and Workspaces

<img src="guide-workspace-dashboard.png" dark-src="guide-workspace-dashboard.png" alt="Workspace with property editor, plot, map, and telemetry dashboard" thumbnail="true"/>

Open Home → Controls gallery → Workspace. The running example combines collapsible widgets, a plot, a map, and Battery/GNSS tiles.

A dashboard groups telemetry tiles. A workspace places larger widgets in left, right, bottom, and
center regions. Combine them by using `DashboardWidget`, which implements `IWorkspaceWidget` and
adds a menu for each tile's density and visibility.

## Register the views

The default core registration includes both families. For explicit control registration:

```c#
using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();
builder.RegisterCore(core => core.RegisterControls(controls =>
    controls.RegisterDashboard().RegisterWorkspace()));
```

`RegisterDashboard()` maps `IDashboard`, `DashboardViewModel`, and `TextTileViewModel` to their views.
`RegisterWorkspace()` maps stack-panel and wrap-panel widget view models. Custom widget content
still needs its own view registration.

## Build a dashboard

The following factory returns an owned dashboard ready for insertion into a workspace. The caller
must attach the dashboard to its view-model tree and dispose it with its page.

```c#
using Asv.Avalonia;
using Material.Icons;

public static class ReceiverDashboard
{
    public static DashboardWidget Create()
    {
        var dashboard = new DashboardWidget("receiver-dashboard")
        {
            Header = "Receiver",
            Position = WorkspaceDock.Right,
            IsVisible = true,
            CanExpand = true,
            IsExpanded = true
        };
        dashboard.Tiles.Add(new TextTileViewModel("navigation")
        {
            Header = "Navigation solution",
            ShortHeader = "NAV",
            Description = "**Solution:** valid\n- Satellites: 18",
            Density = TileDensity.Regular,
            Order = 10,
            IsVisible = true,
            Icon = MaterialIconKind.CrosshairsGps,
            IconColor = AsvColorKind.Info5,
            Text = "RTK",
            SubScriptText = "Fixed",
            TextColor = AsvColorKind.Success,
            StatusText = "18 satellites",
            StatusTextColor = AsvColorKind.Success,
            Progress = double.NaN
        });
        dashboard.Tiles.Add(new TextTileViewModel("link")
        {
            Header = "Connection",
            ShortHeader = "LINK",
            Density = TileDensity.Inline,
            Order = 20,
            IsVisible = true,
            Text = "Online",
            TextColor = AsvColorKind.Success
        });
        return dashboard;
    }
}
```

Use `DashboardViewModel("id")` instead when the dashboard is ordinary page content rather than a
workspace widget. Present either dashboard with a `ContentControl` whose `Content` is the dashboard
view model. Avoid the parameterless constructors at runtime: they are intended for design previews.

## Density, updates, and ordering

`Tiles` is the source collection. `Regular` and `Inline` are derived observable collections consumed
by `DashboardView`. Assign `TileDensity.Regular` for detailed tiles and `TileDensity.Inline` for the
compact strip. Changing `Density` moves a tile to the corresponding collection immediately.

The dashboard inserts tiles according to `Order` when adding or moving them between density groups.
It does not re-sort in response to a later `Order` mutation. Set ordering before insertion.

A `TextTileViewModel` exposes `Text`, `SuperScriptText`, `SubScriptText`, `Units`, `StatusText`, and
separate palette colors for those fields. `StatusIcon` and `StatusColor` provide additional status
indicators. Defaults contain demonstration colors; set the colors that carry meaning in your app.
`Progress` is passed through the telemetry progress converter. `double.NaN` hides the bar; either
infinity selects indeterminate mode; a finite value is passed through unchanged. Values are not
normalized by the converter.

Update bound properties on the UI thread. When consuming background telemetry, marshal its stream
with `ObserveOnUIThreadDispatcher()` before changing tile properties. Retain and dispose the
subscription with the owning view model. Call `MarkUpdated()` after a successful update when the
view should display its update animation.

## Place widgets in a workspace

Given a view model with an `Items` collection of `IWorkspaceWidget`, use the following AXAML inside
its view. Declare the `asv` namespace as `clr-namespace:Asv.Avalonia;assembly=Asv.Avalonia`.

```xml
<asv:Workspace ItemsSource="{Binding Items}" WidgetSpacing="4">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <asv:WorkspacePanel
                MinLeftWidth="250"
                LeftWidth="250"
                CentralWidth="*"
                MinRightWidth="200"
                RightWidth="200"
                BottomHeight="250"
                MinBottomHeight="150" />
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
</asv:Workspace>
```

`Workspace` creates `WorkspaceItem` containers. Widgets expose `Position`, `Header`, `Icon`,
`IsVisible`, `CanExpand`, `IsExpanded`, `Order`, and `MenuView`. `WorkspaceDock` provides `Left`,
`Right`, `Bottom`, and `Center`. `WidgetSpacing` also determines resize-area spacing. Set meaningful
minimum region sizes so a user cannot shrink a widget below its useful content size.

`WorkspaceWidget` is the general headlined widget base. `StackPanelWidgetViewModel` and
`WrapPanelWidgetViewModel` compose child view models using their `ItemsSource`; their constructors
accept an ID and `ILoggerFactory`. `PropertyEditorWidgetViewModel` integrates an editor into this
system. See [Property Editor](property-editor.md) for property composition.

## Lifetime and persistence

A dashboard parents tiles and disposes removed tiles. On disposal it removes density subscriptions
and disposes remaining tiles. A `DashboardWidget` also removes its per-tile menu entries and event
subscriptions. Treat removal as disposal: hide a tile with `IsVisible` when its identity and updates
must survive.

Workspace presentation alone does not establish application ownership. The page that creates widgets
must keep their parents and lifetimes consistent with its view-model tree. Dispose telemetry
subscriptions together with their owners; a hidden widget can otherwise continue receiving updates.

`TileViewModel` registers its `Density` layout entry, loads it when attached to a shell, and saves
subsequent density changes after the initial load. Thus the density menu participates in the normal
page or shell layout store. Visibility is a live property without the same built-in registration;
register it explicitly if your application needs to restore it. Use the workspace dimension helpers
described in [Layout Persistence](layout-persistence.md). Give widgets and tiles stable IDs and
attach them to the view-model tree so the layout manager can identify the same logical objects.

The running example's **Controls Gallery → Workspace** shows region layout and resize behavior.
`WorkspacePageViewModel` and `WorkspacePageView.axaml` provide a complete page, while
`TileDashboardViewModelTest` covers density regrouping, ordering, and dashboard menu behavior.
