using System.Collections.Immutable;

namespace Asv.Avalonia;

internal sealed class ScaledUnitValueFormatter
{
    private readonly ImmutableArray<(IUnitItem Item, double SiValuePerUnit)> _items;

    public ScaledUnitValueFormatter(IEnumerable<IUnitItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        _items =
        [
            .. items
                .Select(item => (Item: item, SiValuePerUnit: Math.Abs(item.ToSi(1.0))))
                .OrderBy(item => item.SiValuePerUnit),
        ];

        if (_items.IsEmpty)
        {
            throw new ArgumentException("At least one unit item is required", nameof(items));
        }

        if (_items.Any(item => !double.IsFinite(item.SiValuePerUnit) || item.SiValuePerUnit <= 0))
        {
            throw new ArgumentException(
                "Unit items must use finite positive conversion factors",
                nameof(items)
            );
        }
    }

    public static ScaledUnitValueFormatter Create(IUnitService unitService, string unitId)
    {
        ArgumentNullException.ThrowIfNull(unitService);
        ArgumentException.ThrowIfNullOrWhiteSpace(unitId);
        var unit = unitService[unitId];
        if (unit is null)
        {
            throw new UnitException($"Unit {unitId} was not found");
        }

        return new ScaledUnitValueFormatter(unit.AvailableUnits.Values);
    }

    public string Print(double value, string? format)
    {
        if (double.IsNaN(value))
        {
            return Units.NotAvailableString;
        }

        var item = SelectItem(value);
        var convertedValue = item.FromSi(value);
        var valueFormat = format ?? GetDefaultFormat(convertedValue);
        var printedValue = item.Print(convertedValue, valueFormat);
        if (format is null)
        {
            printedValue = printedValue.PadRight(4);
        }

        return $"{printedValue} {item.Symbol}";
    }

    private IUnitItem SelectItem(double value)
    {
        var absoluteValue = Math.Abs(value);
        var selected = _items[0].Item;
        foreach (var item in _items)
        {
            if (absoluteValue < item.SiValuePerUnit)
            {
                break;
            }

            selected = item.Item;
        }

        return selected;
    }

    private static string GetDefaultFormat(double value)
    {
        return value != 0 && Math.Abs(value) < 1 ? "F3" : "F0";
    }
}
