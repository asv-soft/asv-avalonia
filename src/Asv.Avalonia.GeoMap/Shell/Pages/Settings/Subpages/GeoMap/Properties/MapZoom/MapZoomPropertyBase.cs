using Asv.Avalonia;
using Asv.Common;
using Asv.Modeling;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.GeoMap;

public abstract class MapZoomPropertyBase : PropertyComboBoxViewModel
{
    private readonly SynchronizedReactiveProperty<int> _modelProperty;

    protected MapZoomPropertyBase(
        string id,
        SynchronizedReactiveProperty<int> modelProperty,
        ILoggerFactory loggerFactory
    )
        : base(id)
    {
        _modelProperty = modelProperty;
        modelProperty
            .Skip(1)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(value => ApplyModelValue(value))
            .DisposeItWith(Disposable);
    }

    protected void SetAvailableValues(IEnumerable<int> values)
    {
        var currentValue = _modelProperty.Value;
        ItemsSource.Clear();

        foreach (var value in values.Distinct().Order())
        {
            ItemsSource.Add(new ZoomLevelItem(value));
        }

        ApplyModelValue(currentValue);
    }

    protected override ValueTask ApplyFromUser(IHeadlinedViewModel item, CancellationToken cancel)
    {
        if (item is not ZoomLevelItem zoom)
        {
            return ValueTask.CompletedTask;
        }

        _modelProperty.Value = zoom.Value;
        return ValueTask.CompletedTask;
    }

    private void ApplyModelValue(int value)
    {
        var item = ItemsSource.OfType<ZoomLevelItem>().FirstOrDefault(x => x.Value == value);
        if (item is not null)
        {
            ApplyValueFromModel(item);
        }
    }

    private sealed class ZoomLevelItem : HeadlinedViewModel
    {
        public ZoomLevelItem(int value)
            : base(value.ToString())
        {
            Value = value;
            Header = value.ToString();
            Description = string.Format(RS.MapZoomProperty_ZoomLevel_Description, value);
        }

        public int Value { get; }
    }
}
