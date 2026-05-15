using Asv.Common;

namespace Asv.Avalonia;

public interface IUnitItem
{
    string UnitItemId { get; }
    string Name { get; }
    string Description { get; }
    string Symbol { get; }
    bool IsInternationalSystemUnit { get; }
    bool IsValid(string? value);
    ValidationResult ValidateValue(string? value);
    double Parse(string? value);
    double ParseToSi(string? value) => ToSi(Parse(value));
    string Print(double value, string? format = null);
    string PrintFromSi(double value, string? format = null) => Print(FromSi(value), format);
    string PrintWithUnits(double value, string? format = null);
    string PrintFromSiWithUnits(double value, string? format = null) =>
        PrintWithUnits(FromSi(value), format);
    double FromSi(double siValue);
    double ToSi(double value);
}
