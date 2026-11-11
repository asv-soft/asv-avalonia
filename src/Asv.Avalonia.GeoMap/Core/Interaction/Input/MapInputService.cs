using Asv.Cfg;
using Asv.Common;
using R3;

namespace Asv.Avalonia.GeoMap;

public sealed class MapInputConfig
{
    // Configuration deserialization may populate nested instances, including init-only records.
    public MapInputBindings Bindings { get; set; } = new();
}

public interface IMapInputService
{
    ReadOnlyReactiveProperty<MapInputBindings> Bindings { get; }
    bool TryAssign(MapInputAction action, MapGesturePreset preset, out MapInputAction? conflict);
    void Reset();
}

/// <summary>Persists map gesture presets and publishes atomic, validated assignments.</summary>
public sealed class MapInputService : AsyncDisposableOnce, IMapInputService
{
    private readonly IConfiguration? _configuration;
    private readonly SynchronizedReactiveProperty<MapInputBindings> _bindings;
    private readonly Lock _sync = new();

    public MapInputService(IConfiguration? configuration = null)
    {
        _configuration = configuration;
        var bindings = configuration?.Get<MapInputConfig>().Bindings;
        _bindings = new SynchronizedReactiveProperty<MapInputBindings>(
            bindings is not null && bindings.IsValid() ? bindings : MapInputBindings.Default
        );
    }

    public ReadOnlyReactiveProperty<MapInputBindings> Bindings => _bindings;

    public bool TryAssign(
        MapInputAction action,
        MapGesturePreset preset,
        out MapInputAction? conflict
    )
    {
        if (!MapInputPresets.GetPresets(action).Contains(preset))
        {
            throw new ArgumentOutOfRangeException(nameof(preset));
        }

        using (_sync.EnterScope())
        {
            conflict = _bindings.Value.FindConflict(action, preset);
            if (conflict is not null)
            {
                return false;
            }

            Apply(_bindings.Value.With(action, preset));
            return true;
        }
    }

    public void Reset()
    {
        using (_sync.EnterScope())
        {
            Apply(MapInputBindings.Default);
        }
    }

    private void Apply(MapInputBindings bindings)
    {
        if (_bindings.Value == bindings)
        {
            return;
        }

        _configuration?.Set(new MapInputConfig { Bindings = bindings });
        _bindings.Value = bindings;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _bindings.Dispose();
        }

        base.Dispose(disposing);
    }
}
