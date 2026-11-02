using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using R3;

namespace Asv.Avalonia.GeoMap;

/// <summary>Edits one map action using a predefined complete pointer gesture.</summary>
public sealed class MapInputGestureProperty : PropertyComboBoxViewModel
{
    private readonly IMapInputService _service;

    public MapInputGestureProperty(MapInputAction action, IMapInputService service)
        : base($"map-input-{action}", enableUndo: false)
    {
        ArgumentNullException.ThrowIfNull(service);
        Action = action;
        _service = service;
        Header = MapInputStrings.ActionName(action);
        Description = MapInputStrings.Get($"{action}_Description");
        Icon = MaterialIconKind.Mouse;

        foreach (var preset in MapInputPresets.GetPresets(action))
        {
            ItemsSource.Add(new PresetItem(preset).DisposeItWith(Disposable));
        }

        service.Bindings
            .ObserveOnUIThreadDispatcher()
            .Subscribe(bindings => ApplyBindings(bindings))
            .DisposeItWith(Disposable);
        ApplyBindings(service.Bindings.CurrentValue);
    }

    public MapInputAction Action { get; }

    protected override ValueTask ApplyFromUser(IHeadlinedViewModel item, CancellationToken cancel)
    {
        cancel.ThrowIfCancellationRequested();
        if (item is not PresetItem preset)
        {
            return ValueTask.CompletedTask;
        }

        if (!_service.TryAssign(Action, preset.Preset, out var conflict))
        {
            var message = conflict is { } action
                ? string.Format(
                    MapInputStrings.Get("Conflict"),
                    MapInputStrings.ActionName(action)
                )
                : MapInputStrings.Get("Unavailable");
            throw new InvalidOperationException(message);
        }

        return ValueTask.CompletedTask;
    }

    internal void RefreshFromService()
    {
        ApplyBindings(_service.Bindings.CurrentValue);
    }

    private void ApplyBindings(MapInputBindings bindings)
    {
        var preset = bindings.Get(Action);
        ApplyValueFromModel(ItemsSource.OfType<PresetItem>().First(item => item.Preset == preset));
    }

    private sealed class PresetItem : HeadlinedViewModel
    {
        public PresetItem(MapGesturePreset preset)
            : base(preset.ToString())
        {
            Preset = preset;
            Header = MapInputStrings.Get($"Gesture_{preset}");
        }

        public MapGesturePreset Preset { get; }
    }
}
