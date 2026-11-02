using Asv.Avalonia.GeoMap;
using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Xunit;

namespace Asv.Avalonia.Test;

[Collection(WorkspaceUiTestCollection.Name)]
public class MapInputControlTest(WorkspaceUiTestFixture fixture)
{
    [Fact]
    public Task Defaults_CtrlMovesMarkerWithoutChangingAltitude_AndShiftSelectsRectangle() => Run(() =>
    {
        using var scene = new Scene();
        var original = scene.Marker.Location;
        scene.Window.MouseDown(new Point(20, 20), MouseButton.Left, RawInputModifiers.Control);
        Assert.Equal(DragState.DragSelection, scene.Map.DragState);
        scene.Window.MouseMove(new Point(60, 40), RawInputModifiers.LeftMouseButton | RawInputModifiers.Control);
        scene.Window.MouseUp(new Point(60, 40), MouseButton.Left, RawInputModifiers.Control);
        Assert.NotEqual(original.Latitude, scene.Marker.Location.Latitude);
        Assert.Equal(300, scene.Marker.Location.Altitude);
        scene.Window.MouseDown(new Point(160, 100), MouseButton.Left, RawInputModifiers.Shift);
        Assert.Equal(DragState.SelectRectangle, scene.Map.DragState);
        scene.Window.MouseMove(new Point(5, 5), RawInputModifiers.LeftMouseButton | RawInputModifiers.Shift);
        scene.Window.MouseUp(new Point(5, 5), MouseButton.Left, RawInputModifiers.Shift);
        Assert.Same(scene.Marker, scene.Map.SelectedItem);
    });

    [Fact]
    public Task AltPreset_MovesMarker_WhileOldCtrlGestureDoesNot() => Run(() =>
    {
        using var scene = new Scene();
        scene.Map.InputBindings = MapInputBindings.Default with { MoveMarkers = MapGesturePreset.AltLeftDrag };
        var original = scene.Marker.Location;
        scene.Window.MouseDown(new Point(20, 20), MouseButton.Left, RawInputModifiers.Control);
        Assert.Equal(DragState.DragMap, scene.Map.DragState);
        scene.Window.MouseMove(new Point(35, 35), RawInputModifiers.LeftMouseButton | RawInputModifiers.Control);
        scene.Window.MouseUp(new Point(35, 35), MouseButton.Left, RawInputModifiers.Control);
        Assert.Equal(original, scene.Marker.Location);
        scene.Window.MouseDown(new Point(20, 20), MouseButton.Left, RawInputModifiers.Alt);
        Assert.Equal(DragState.DragSelection, scene.Map.DragState);
        scene.Window.MouseMove(new Point(60, 40), RawInputModifiers.LeftMouseButton | RawInputModifiers.Alt);
        scene.Window.MouseUp(new Point(60, 40), MouseButton.Left, RawInputModifiers.Alt);
        Assert.NotEqual(original.Longitude, scene.Marker.Location.Longitude);
        Assert.Equal(300, scene.Marker.Location.Altitude);
    });

    [Fact]
    public Task MiddlePan_PreservesPlainLeftMapClickAndDoesNotPanOnLeftDrag() => Run(() =>
    {
        using var scene = new Scene();
        scene.Map.InputBindings = MapInputBindings.Default with { Pan = MapGesturePreset.MiddleDrag };
        var clicks = 0;
        scene.Map.MapClick += (_, _) => clicks++;
        var center = scene.Map.CenterMap;
        scene.Window.MouseDown(new Point(150, 150), MouseButton.Left);
        scene.Window.MouseUp(new Point(150, 150), MouseButton.Left);
        Assert.Equal(1, clicks);
        scene.Window.MouseDown(new Point(150, 150), MouseButton.Left);
        scene.Window.MouseMove(new Point(200, 180), RawInputModifiers.LeftMouseButton);
        scene.Window.MouseUp(new Point(200, 180), MouseButton.Left);
        Assert.Equal(center, scene.Map.CenterMap);
        Assert.Equal(1, clicks);
        scene.Window.MouseDown(new Point(150, 150), MouseButton.Middle);
        Assert.Equal(DragState.DragMap, scene.Map.DragState);
        scene.Window.MouseMove(new Point(200, 180), RawInputModifiers.MiddleMouseButton);
        scene.Window.MouseUp(new Point(200, 180), MouseButton.Middle);
        Assert.NotEqual(center, scene.Map.CenterMap);
        Assert.Equal(1, clicks);
    });

    [Fact]
    public Task ShiftDragZoom_ReplacesWheel_AccumulatesVerticalStepsAndStopsOnRelease() => Run(() =>
    {
        using var scene = new Scene();
        scene.Map.InputBindings = MapInputBindings.Default with
        {
            SelectRectangle = MapGesturePreset.AltLeftDrag,
            Zoom = MapGesturePreset.ShiftLeftDrag,
        };
        var center = scene.Map.CenterMap;
        scene.Window.MouseWheel(new Point(150, 150), new Vector(0, 1));
        Assert.Equal(10, scene.Map.Zoom);
        scene.Window.MouseDown(new Point(150, 180), MouseButton.Left, RawInputModifiers.Shift);
        Assert.Equal(DragState.Zoom, scene.Map.DragState);
        scene.Window.MouseMove(new Point(150, 160), RawInputModifiers.LeftMouseButton | RawInputModifiers.Shift);
        Assert.Equal(10, scene.Map.Zoom);
        scene.Window.MouseMove(new Point(150, 100), RawInputModifiers.LeftMouseButton | RawInputModifiers.Shift);
        Assert.Equal(12, scene.Map.Zoom);
        Assert.Equal(center, scene.Map.CenterMap);
        scene.Window.MouseUp(new Point(150, 100), MouseButton.Left, RawInputModifiers.Shift);
        Assert.Equal(DragState.None, scene.Map.DragState);
        scene.Window.MouseMove(new Point(150, 50));
        Assert.Equal(12, scene.Map.Zoom);
    });

    [Fact]
    public Task ShiftWheelZoom_RequiresShift_AndHonorsWheelDisableAndZoomBounds() => Run(() =>
    {
        using var scene = new Scene();
        scene.Map.InputBindings = MapInputBindings.Default with { Zoom = MapGesturePreset.ShiftWheel };
        scene.Window.MouseWheel(new Point(150, 150), new Vector(0, 1));
        Assert.Equal(10, scene.Map.Zoom);
        scene.Window.MouseWheel(new Point(150, 150), new Vector(0, 1), RawInputModifiers.Shift);
        Assert.Equal(11, scene.Map.Zoom);
        scene.Map.EnableWheelZoom = false;
        scene.Window.MouseWheel(new Point(150, 150), new Vector(0, 1), RawInputModifiers.Shift);
        Assert.Equal(11, scene.Map.Zoom);
        scene.Map.EnableWheelZoom = true;
        scene.Map.MaxZoom = 11;
        scene.Window.MouseWheel(new Point(150, 150), new Vector(0, 1), RawInputModifiers.Shift);
        Assert.Equal(11, scene.Map.Zoom);
    });

    [Fact]
    public Task ReadOnlyMarker_DoesNotMoveWithAssignedGesture() => Run(() =>
    {
        using var scene = new Scene();
        scene.Marker.IsReadOnly = true;
        var original = scene.Marker.Location;
        scene.Window.MouseDown(new Point(20, 20), MouseButton.Left, RawInputModifiers.Control);
        scene.Window.MouseMove(new Point(80, 60), RawInputModifiers.LeftMouseButton | RawInputModifiers.Control);
        scene.Window.MouseUp(new Point(80, 60), MouseButton.Left, RawInputModifiers.Control);
        Assert.Equal(original, scene.Marker.Location);
    });

    [Fact]
    public Task ChangingBindingsDuringDrag_ReleasesCaptureAndStopsOldGesture() => Run(() =>
    {
        using var scene = new Scene();
        scene.Window.MouseDown(new Point(150, 150), MouseButton.Left);
        Assert.Equal(DragState.DragMap, scene.Map.DragState);
        scene.Map.InputBindings = MapInputBindings.Default with { Pan = MapGesturePreset.MiddleDrag };
        Assert.Equal(DragState.None, scene.Map.DragState);
        var center = scene.Map.CenterMap;
        scene.Window.MouseMove(new Point(220, 180), RawInputModifiers.LeftMouseButton);
        scene.Window.MouseUp(new Point(220, 180), MouseButton.Left);
        Assert.Equal(center, scene.Map.CenterMap);
    });

    [Fact]
    public Task MapViewModels_ReceiveSettingsChanges_AndDisposeTheirSubscriptions() => Run(() =>
    {
        var mapService = new NullMapService();
        using var first = new MapViewModel("first", mapService);
        using var second = new MapViewModel("second", mapService);
        Assert.True(mapService.Input.TryAssign(MapInputAction.MoveMarkers, MapGesturePreset.MetaLeftDrag, out _));
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(MapGesturePreset.MetaLeftDrag, first.InputBindings.Value.MoveMarkers);
        Assert.Equal(first.InputBindings.Value, second.InputBindings.Value);
        first.Dispose();
        Assert.True(mapService.Input.TryAssign(MapInputAction.MoveMarkers, MapGesturePreset.AltLeftDrag, out _));
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(MapGesturePreset.MetaLeftDrag, first.InputBindings.Value.MoveMarkers);
        Assert.Equal(MapGesturePreset.AltLeftDrag, second.InputBindings.Value.MoveMarkers);
    });

    [Fact]
    public Task SettingsGesture_ConflictRollsBackSelection_AndResetClearsTheError() => Run(() =>
    {
        using var service = new MapInputService();
        using var property = new MapInputGestureProperty(MapInputAction.Zoom, service);
        var original = property.SelectedItem.Value;
        var conflicting = property.ItemsSource.Single(item =>
            item.Id.ToString() == MapGesturePreset.ShiftLeftDrag.ToString());

        property.SelectItem(conflicting).GetAwaiter().GetResult();

        Assert.Same(original, property.SelectedItem.Value);
        Assert.False(string.IsNullOrWhiteSpace(property.ErrorMessage));
        Assert.Equal(MapGesturePreset.Wheel, service.Bindings.CurrentValue.Zoom);
        Assert.Equal(MapGesturePreset.ShiftLeftDrag, service.Bindings.CurrentValue.SelectRectangle);

        Assert.True(service.TryAssign(MapInputAction.Zoom, MapGesturePreset.AltLeftDrag, out _));
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(MapGesturePreset.AltLeftDrag.ToString(), property.SelectedItem.Value!.Id.ToString());
        Assert.Null(property.ErrorMessage);

        service.Reset();
        Dispatcher.UIThread.RunJobs();
        Assert.Same(original, property.SelectedItem.Value);
        Assert.Null(property.ErrorMessage);
    });

    [Fact]
    public Task SettingsGesture_AppliesToOpenMap_AndAllowsShiftZoomAfterFreeingItsGesture() => Run(() =>
    {
        var mapService = new NullMapService();
        using var map = new MapViewModel("map", mapService);
        using var selection = new MapInputGestureProperty(MapInputAction.SelectRectangle, mapService.Input);
        using var zoom = new MapInputGestureProperty(MapInputAction.Zoom, mapService.Input);

        selection.SelectItem(selection.ItemsSource.Single(item =>
            item.Id.ToString() == MapGesturePreset.AltLeftDrag.ToString())).GetAwaiter().GetResult();
        zoom.SelectItem(zoom.ItemsSource.Single(item =>
            item.Id.ToString() == MapGesturePreset.ShiftLeftDrag.ToString())).GetAwaiter().GetResult();
        Dispatcher.UIThread.RunJobs();

        Assert.Null(selection.ErrorMessage);
        Assert.Null(zoom.ErrorMessage);
        Assert.Equal(MapGesturePreset.AltLeftDrag, map.InputBindings.Value.SelectRectangle);
        Assert.Equal(MapGesturePreset.ShiftLeftDrag, map.InputBindings.Value.Zoom);
        Assert.False(MapInputPresets.MatchesWheel(map.InputBindings.Value.Zoom, KeyModifiers.None));
    });

    [Theory]
    [InlineData(MapInputAction.Zoom)]
    [InlineData(MapInputAction.SelectRectangle)]
    public Task PlainLeftDragPreset_PreservesStationaryClicksForMapTools(MapInputAction action) => Run(() =>
    {
        using var scene = new Scene();
        scene.Map.InputBindings = (MapInputBindings.Default with
        {
            Pan = MapGesturePreset.MiddleDrag,
        }).With(action, MapGesturePreset.LeftDrag);
        scene.Map.CenterMap = new GeoPoint(54, 54, 0);
        scene.Map.SelectedItem = scene.Marker;
        var center = scene.Map.CenterMap;
        var clicks = 0;
        scene.Map.MapClick += (_, _) => clicks++;

        scene.Window.MouseDown(new Point(150, 150), MouseButton.Left);
        scene.Window.MouseUp(new Point(150, 150), MouseButton.Left);

        Assert.Equal(1, clicks);
        Assert.Equal(10, scene.Map.Zoom);
        Assert.Equal(center, scene.Map.CenterMap);
    });

    [Fact]
    public Task ShiftWheelZoom_AcceptsHorizontalWheelDelta() => Run(() =>
    {
        using var scene = new Scene();
        scene.Map.InputBindings = MapInputBindings.Default with { Zoom = MapGesturePreset.ShiftWheel };

        scene.Window.MouseWheel(new Point(150, 150), new Vector(1, 0), RawInputModifiers.Shift);
        Assert.Equal(11, scene.Map.Zoom);
        scene.Window.MouseWheel(new Point(150, 150), new Vector(-1, 0), RawInputModifiers.Shift);
        Assert.Equal(10, scene.Map.Zoom);
    });

    private Task Run(Action action) => fixture.Session.Dispatch(action, TestContext.Current.CancellationToken);

    private sealed class Scene : IDisposable
    {
        public Scene()
        {
            Marker = new MapItem
            {
                Width = 40,
                Height = 40,
                Location = new GeoPoint(53, 53, 300),
                Template = new FuncControlTemplate<MapItem>((_, _) => new Border { Background = Brushes.Red }),
            };
            Map = new MapItemsControl
            {
                CenterMap = new GeoPoint(53, 53, 0),
                Zoom = 10,
                MinZoom = 1,
                MaxZoom = 19,
                Template = new FuncControlTemplate<MapItemsControl>((_, scope) =>
                {
                    var presenter = new ItemsPresenter
                    {
                        Name = "PART_ItemsPresenter",
                        ItemsPanel = new FuncTemplate<Panel>(() => new Canvas()),
                    };
                    scope.Register(presenter.Name, presenter);
                    return new Border { Background = Brushes.Transparent, Child = presenter };
                }),
            };
            Map.Items.Add(Marker);
            Window = new Window { Width = 400, Height = 300, Content = Map };
            Window.Show();
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
        }

        public Window Window { get; }
        public MapItemsControl Map { get; }
        public MapItem Marker { get; }

        public void Dispose() => Window.Close();
    }
}
