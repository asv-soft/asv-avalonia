namespace Asv.Avalonia;

public interface IUnitItem
{
    string UnitItemId { get; }
    string Name { get; }
    string Description { get; }
    string Symbol { get; }
    bool IsInternationalSystemUnit { get; }
    bool IsValid(string? value);
    string? GetValidationErrorMessage(string? value);
    double Parse(string? input);
    double ParseToSi(string? value) => ToSi(Parse(value));
    string Print(double value, string? format = null);
    string PrintFromSi(double value, string? format = null) => Print(FromSi(value));
    string PrintWithUnits(double value, string? format = null);
    string PrintFromSiWithUnits(double value, string? format = null) =>
        PrintWithUnits(FromSi(value));
    double FromSi(double siValue);
    double ToSi(double value);
}
