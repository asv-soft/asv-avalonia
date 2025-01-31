using System.Collections.Immutable;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public static class Units
{
    public static readonly char[] Kilo = ['K', 'k', 'К', 'k'];
    public static readonly char[] Mega = ['M', 'm', 'М', 'м'];
    public static readonly char[] Giga = ['B', 'b', 'G', 'g', 'Г', 'г'];
    public static IEnumerable<char> All => Kilo.Concat(Mega).Concat(Giga);
}

public interface IUnitService
{
    IReadOnlyDictionary<string, IUnit> Units { get; }
    IUnit? this[string unit] => Units.GetValueOrDefault(unit);
    IUnitItem? this[string unit, string item] => this[unit]?[item];
}