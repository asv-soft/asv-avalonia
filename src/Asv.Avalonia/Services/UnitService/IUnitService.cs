using System.Collections.Immutable;
using System.Globalization;
using R3;

namespace Asv.Avalonia;

public static class WellKnownUnits
{
    public static readonly char[] Kilo = ['K', 'k', 'К', 'k'];
    public static readonly char[] Mega = ['M', 'm', 'М', 'м'];
    public static readonly char[] Giga = ['B', 'b', 'G', 'g', 'Г', 'г'];

    public const string Distance = "asv:unit/distance";
    public const string DistanceMeters = $"asv:unit.item/meters";
}

public interface IUnitService
{
    IUnit? this[string id] { get; }
    IUnit GetUnit(string unitId);
}

public interface IUnit
{
    string Name { get; }
    string Description { get; }
    string UnitId { get; }
    bool IsInternationalSystemUnit { get; }
    IEnumerable<IUnitItem> AvailableUnits { get; }
    ReadOnlyReactiveProperty<IUnitItem> Current { get; }
    IUnitItem InternationalSystemUnit { get; }
}

public interface IUnitItem
{
    IUnit Parent { get; }
    string Id { get; }
    string Name { get; }
    string Description { get; }
    string Symbol { get; }
    bool IsInternationalSystemUnit { get; }
    bool IsValid(string? value);
    string? GetValidationErrorMessage(string? value);
    double Parse(string? input);
    double ParseToSi(string? value) => ConvertToSi(Parse(value));
    string Print(double value, string? format = null);
    string PrintFromSi(double value, string? format = null) => Print(ConvertFromSi(value));
    string PrintWithUnits(double value, string? format = null);
    string PrintFromSiWithUnits(double value, string? format = null) =>
        PrintWithUnits(ConvertFromSi(value));
    double ConvertFromSi(double siValue);
    double ConvertToSi(double value);
}

public class SimpleUnitItem : IUnitItem
{
    public required IUnit Parent { get; set; }
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Symbol { get; set; }
    public required bool IsInternationalSystemUnit { get; set; }

    public virtual bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        value = value.Replace(',', '.');
        return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
    }

    public string? GetValidationErrorMessage(string? value)
    {
        return null;
    }

    public double Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return double.NaN;
        }

        input = input.Trim().Replace(',', '.');

        double multiplier = 1;
        if (WellKnownUnits.Kilo.Any(x => input.EndsWith(x)))
        {
            multiplier = 1_000;
            input = input[..^1]; // Убираем последний символ "K"
        }
        else if (input.EndsWith("M", StringComparison.InvariantCultureIgnoreCase))
        {
            multiplier = 1_000_000;
            input = input[..^1];
        }
        else if (input.EndsWith("B", StringComparison.InvariantCultureIgnoreCase))
        {
            multiplier = 1_000_000_000;
            input = input[..^1];
        }

        return Double.NaN;
    }

    public string Print(double value, string? format = null)
    {
        throw new NotImplementedException();
    }

    public string PrintWithUnits(double value, string? format = null)
    {
        throw new NotImplementedException();
    }

    public double ConvertFromSi(double siValue)
    {
        throw new NotImplementedException();
    }

    public double ConvertToSi(double value)
    {
        throw new NotImplementedException();
    }
}

public class ReactiveUnitProperty
{
    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;

    public ReactiveUnitProperty(IUnit unit)
    {
        Value = new BindableReactiveProperty<double>(double.NaN);
        StringValue = new BindableReactiveProperty<string>(string.Empty);
        Unit = unit;
        _sub1 = StringValue.Subscribe(StringToValue);
        _sub2 = Value.Subscribe(ValueToString);
    }

    private void ValueToString(double value)
    {
        throw new NotImplementedException();
    }

    private void StringToValue(string value)
    {
        throw new NotImplementedException();
    }

    public IUnit Unit { get; }

    public BindableReactiveProperty<double> Value { get; }

    public BindableReactiveProperty<string> StringValue { get; }
}
