namespace Asv.Avalonia;

public static class Units
{
    public const char DecimalSeparator = '.';
    public const string NotAvailableString = "N/A";

    public static readonly char[] Kilo = ['K', 'k', 'К', 'k'];
    public static readonly char[] Mega = ['M', 'm', 'М', 'м'];
    public static readonly char[] Giga = ['B', 'b', 'G', 'g', 'Г', 'г'];
    public static IEnumerable<char> All => Kilo.Concat(Mega).Concat(Giga);

    public static void PrintSplitString(
        this IUnitItem item,
        double value,
        string? format,
        int fractionDigits,
        out string intStr,
        out string fracStr
    )
    {
        if (double.IsNaN(value))
        {
            intStr = NotAvailableString;
            fracStr = string.Empty;
            return;
        }
        var origin = item.PrintFromSi(value, format);
        if (origin.Length <= fractionDigits)
        {
            fracStr = origin;
            intStr = string.Empty;
        }
        else
        {
            fracStr = origin[^fractionDigits..];
            intStr = origin[..^fractionDigits];
        }
    }

    public static void PrintSplitString(
        this IUnitItem item,
        double value,
        string? format,
        out string intStr,
        out string fracStr
    )
    {
        if (double.IsNaN(value))
        {
            intStr = NotAvailableString;
            fracStr = string.Empty;
            return;
        }
        var origin = item.PrintFromSi(value, format);
        var dotIndex = origin.IndexOf(DecimalSeparator);
        if (dotIndex > 0)
        {
            fracStr = $"{DecimalSeparator}{origin[(dotIndex + 1)..]}";
            intStr = origin[..dotIndex];
        }
        else
        {
            fracStr = string.Empty;
            intStr = origin;
        }
    }
}

public interface IUnitService
{
    IReadOnlyDictionary<string, IUnit> Units { get; }
    IUnit? this[string unit] => Units.GetValueOrDefault(unit);
    IUnitItem? this[string unit, string item] => this[unit]?[item];
}
