using Asv.Avalonia.GeoMap;
using Asv.Cfg;
using Avalonia.Input;
using R3;
using Xunit;

namespace Asv.Avalonia.Test;

public class MapInputServiceTest
{
    [Fact]
    public void LoadingCustomConfiguration_DoesNotMutateSharedDefaults()
    {
        var config = new InMemoryConfiguration();
        config.Set(new MapInputConfig
        {
            Bindings = new MapInputBindings { Pan = MapGesturePreset.MiddleDrag },
        });

        using var custom = new MapInputService(config);
        using var fresh = new MapInputService();

        Assert.Equal(MapGesturePreset.MiddleDrag, custom.Bindings.CurrentValue.Pan);
        Assert.Equal(MapGesturePreset.LeftDrag, MapInputBindings.Default.Pan);
        Assert.Equal(new MapInputBindings(), fresh.Bindings.CurrentValue);
    }

    [Fact]
    public void Defaults_RetainLegacyMapGestures()
    {
        using var service = new MapInputService();
        var bindings = service.Bindings.CurrentValue;

        Assert.Equal(MapGesturePreset.LeftDrag, bindings.Pan);
        Assert.Equal(MapGesturePreset.ControlLeftDrag, bindings.MoveMarkers);
        Assert.Equal(MapGesturePreset.ShiftLeftDrag, bindings.SelectRectangle);
        Assert.Equal(MapGesturePreset.Wheel, bindings.Zoom);
        Assert.True(bindings.IsValid());
    }

    [Fact]
    public void DisabledAssignments_CanBeSharedByEveryAction()
    {
        using var service = new MapInputService();

        foreach (var action in Enum.GetValues<MapInputAction>())
        {
            Assert.True(service.TryAssign(action, MapGesturePreset.None, out var conflict));
            Assert.Null(conflict);
        }

        Assert.All(Enum.GetValues<MapInputAction>(), action =>
            Assert.Equal(MapGesturePreset.None, service.Bindings.CurrentValue.Get(action)));
        Assert.True(service.Bindings.CurrentValue.IsValid());
        Assert.Null(MapInputPresets.ResolveDrag(
            service.Bindings.CurrentValue, MouseButton.Left, KeyModifiers.Control | KeyModifiers.Shift, true));
        Assert.False(MapInputPresets.MatchesWheel(service.Bindings.CurrentValue.Zoom, KeyModifiers.None));
    }

    [Fact]
    public void TryAssign_ConflictPreservesBindingsPersistenceAndNotifications()
    {
        var config = new InMemoryConfiguration();
        var initial = MapInputBindings.Default with { Pan = MapGesturePreset.MiddleDrag };
        config.Set(new MapInputConfig { Bindings = initial });
        using var service = new MapInputService(config);
        var observed = new List<MapInputBindings>();
        using var subscription = service.Bindings.Skip(1).Subscribe(observed.Add);

        var accepted = service.TryAssign(
            MapInputAction.Zoom, MapGesturePreset.ControlLeftDrag, out var conflict);

        Assert.False(accepted);
        Assert.Equal(MapInputAction.MoveMarkers, conflict);
        Assert.Equal(initial, service.Bindings.CurrentValue);
        Assert.Equal(initial, config.Get<MapInputConfig>().Bindings);
        Assert.Empty(observed);
    }

    [Fact]
    public void TryAssign_CustomGesturePersistsAndReloads()
    {
        var config = new InMemoryConfiguration();
        using (var service = new MapInputService(config))
        {
            Assert.True(service.TryAssign(
                MapInputAction.MoveMarkers, MapGesturePreset.MetaLeftDrag, out var conflict));
            Assert.Null(conflict);
        }

        using var reopened = new MapInputService(config);
        Assert.Equal(MapGesturePreset.MetaLeftDrag, reopened.Bindings.CurrentValue.MoveMarkers);
        Assert.Equal(MapGesturePreset.MetaLeftDrag, config.Get<MapInputConfig>().Bindings.MoveMarkers);
        Assert.Equal(MapGesturePreset.LeftDrag, reopened.Bindings.CurrentValue.Pan);
    }

    [Fact]
    public void Reset_RestoresAndPersistsEveryDefault()
    {
        var config = new InMemoryConfiguration();
        using var service = new MapInputService(config);
        Assert.True(service.TryAssign(MapInputAction.Pan, MapGesturePreset.MiddleDrag, out _));
        Assert.True(service.TryAssign(MapInputAction.Zoom, MapGesturePreset.ShiftWheel, out _));

        service.Reset();

        Assert.Equal(MapInputBindings.Default, service.Bindings.CurrentValue);
        Assert.Equal(MapInputBindings.Default, config.Get<MapInputConfig>().Bindings);
        using var reopened = new MapInputService(config);
        Assert.Equal(MapInputBindings.Default, reopened.Bindings.CurrentValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void InvalidStoredBindings_RestoreUsableDefaults(int invalidCase)
    {
        var invalid = invalidCase switch
        {
            0 => MapInputBindings.Default with { Pan = (MapGesturePreset)999 },
            1 => MapInputBindings.Default with { Pan = MapGesturePreset.ControlLeftDrag },
            _ => MapInputBindings.Default with { Pan = MapGesturePreset.Wheel },
        };
        var config = new InMemoryConfiguration();
        config.Set(new MapInputConfig { Bindings = invalid });

        using var service = new MapInputService(config);

        Assert.Equal(MapInputBindings.Default, service.Bindings.CurrentValue);
        Assert.True(service.Bindings.CurrentValue.IsValid());
    }

    [Fact]
    public void TryAssign_WheelForDragActionThrowsWithoutChangingState()
    {
        var config = new InMemoryConfiguration();
        config.Set(new MapInputConfig());
        using var service = new MapInputService(config);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            service.TryAssign(MapInputAction.MoveMarkers, MapGesturePreset.Wheel, out _));

        Assert.Equal(MapInputBindings.Default, service.Bindings.CurrentValue);
        Assert.Equal(MapInputBindings.Default, config.Get<MapInputConfig>().Bindings);
    }

    [Fact]
    public void Assignments_PublishSnapshotOnlyAfterItHasBeenPersisted()
    {
        var config = new InMemoryConfiguration();
        using var service = new MapInputService(config);
        var observed = new List<MapInputBindings>();
        using var subscription = service.Bindings.Skip(1).Subscribe(bindings =>
        {
            Assert.Equal(bindings, config.Get<MapInputConfig>().Bindings);
            observed.Add(bindings);
        });

        Assert.True(service.TryAssign(MapInputAction.Zoom, MapGesturePreset.ShiftWheel, out _));
        Assert.True(service.TryAssign(MapInputAction.Zoom, MapGesturePreset.ShiftWheel, out _));

        var snapshot = Assert.Single(observed);
        Assert.Equal(MapGesturePreset.ShiftWheel, snapshot.Zoom);
        Assert.Equal(MapInputBindings.Default.Pan, snapshot.Pan);
    }

    [Theory]
    [InlineData(KeyModifiers.None, false)]
    [InlineData(KeyModifiers.Control, false)]
    [InlineData(KeyModifiers.Shift, true)]
    [InlineData(KeyModifiers.Shift | KeyModifiers.Control, true)]
    public void ShiftWheelZoom_RequiresShiftAndNeverMatchesDrag(KeyModifiers modifiers, bool matches)
    {
        Assert.Equal(matches, MapInputPresets.MatchesWheel(MapGesturePreset.ShiftWheel, modifiers));
        Assert.False(MapInputPresets.MatchesDrag(MapGesturePreset.ShiftWheel, MouseButton.Left, modifiers));
    }

    [Theory]
    [InlineData(KeyModifiers.None, MapInputAction.Pan)]
    [InlineData(KeyModifiers.Control, MapInputAction.MoveMarkers)]
    [InlineData(KeyModifiers.Shift, MapInputAction.SelectRectangle)]
    [InlineData(KeyModifiers.Control | KeyModifiers.Shift, MapInputAction.SelectRectangle)]
    public void ResolveDrag_DefaultsRetainLegacyModifierPriority(KeyModifiers modifiers, MapInputAction expected)
    {
        Assert.Equal(expected, MapInputPresets.ResolveDrag(
            MapInputBindings.Default, MouseButton.Left, modifiers, hasMarker: true));
    }

    [Fact]
    public void ResolveDrag_ShiftDragZoomReplacesWheelAndDoesNotConsumePlainDrag()
    {
        var bindings = MapInputBindings.Default with
        {
            SelectRectangle = MapGesturePreset.AltLeftDrag,
            Zoom = MapGesturePreset.ShiftLeftDrag,
        };

        Assert.True(bindings.IsValid());
        Assert.Equal(MapInputAction.Zoom, MapInputPresets.ResolveDrag(
            bindings, MouseButton.Left, KeyModifiers.Shift, hasMarker: false));
        Assert.Equal(MapInputAction.Pan, MapInputPresets.ResolveDrag(
            bindings, MouseButton.Left, KeyModifiers.None, hasMarker: false));
        Assert.False(MapInputPresets.MatchesWheel(bindings.Zoom, KeyModifiers.None));
        Assert.False(MapInputPresets.MatchesWheel(bindings.Zoom, KeyModifiers.Shift));
    }

    [Fact]
    public void ResolveDrag_MiddlePanAndCommandMarkerDragUseTheirAssignedInputs()
    {
        var bindings = MapInputBindings.Default with
        {
            Pan = MapGesturePreset.MiddleDrag,
            MoveMarkers = MapGesturePreset.MetaLeftDrag,
        };

        Assert.Equal(MapInputAction.Pan, MapInputPresets.ResolveDrag(
            bindings, MouseButton.Middle, KeyModifiers.None, hasMarker: false));
        Assert.Equal(MapInputAction.MoveMarkers, MapInputPresets.ResolveDrag(
            bindings, MouseButton.Left, KeyModifiers.Meta, hasMarker: true));
        Assert.Null(MapInputPresets.ResolveDrag(
            bindings, MouseButton.Left, KeyModifiers.Control, hasMarker: true));
        Assert.Null(MapInputPresets.ResolveDrag(
            bindings, MouseButton.Left, KeyModifiers.Meta, hasMarker: false));
    }

    [Fact]
    public void ResolveDrag_OptInModifierlessMarkerDragStillHonorsDisabledAction()
    {
        Assert.Equal(MapInputAction.MoveMarkers, MapInputPresets.ResolveDrag(
            MapInputBindings.Default, MouseButton.Left, KeyModifiers.None,
            hasMarker: true, canDragWithoutModifier: true));
        var disabled = MapInputBindings.Default with { MoveMarkers = MapGesturePreset.None };
        Assert.Equal(MapInputAction.Pan, MapInputPresets.ResolveDrag(
            disabled, MouseButton.Left, KeyModifiers.None,
            hasMarker: true, canDragWithoutModifier: true));
    }
}
