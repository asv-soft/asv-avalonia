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

/// <summary>
/// Represents a registry of all registered measurement quantities.
/// </summary>
public interface IUnitService
{
    /// <summary>
    /// Gets all registered quantities keyed by <see cref="IUnit.UnitId"/>.
    /// </summary>
    IReadOnlyDictionary<string, IUnit> Units { get; }

    /// <summary>
    /// Gets a registered quantity by identifier.
    /// </summary>
    /// <param name="unit">The quantity identifier.</param>
    /// <returns>The registered quantity, or <see langword="null"/> when it is not found.</returns>
    IUnit? this[string unit] => Units.GetValueOrDefault(unit);

    /// <summary>
    /// Resolves a specific unit item within a quantity.
    /// </summary>
    /// <param name="unit">The quantity identifier.</param>
    /// <param name="item">The unit item identifier.</param>
    /// <returns>The unit item, or <see langword="null"/> when the quantity is not registered.</returns>
    IUnitItem? this[string unit, string item] => this[unit]?[item];

    /// <summary>
    /// Gets a required quantity and verifies its concrete type.
    /// </summary>
    /// <typeparam name="TUnit">The expected quantity type.</typeparam>
    /// <param name="unitId">The quantity identifier.</param>
    /// <returns>The registered quantity.</returns>
    /// <exception cref="UnitException">The quantity is not registered or has a different type.</exception>
    public TUnit GetRequiredUnitOfType<TUnit>(string unitId)
        where TUnit : IUnit
    {
        var raw = Units.GetValueOrDefault(unitId);

        if (raw is TUnit typed)
        {
            return typed;
        }

        throw new UnitException($"Unit {unitId} is not of type {typeof(TUnit)}");
    }
}
