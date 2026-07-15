using Material.Icons;
using R3;

namespace Asv.Avalonia;

/// <summary>
/// Represents a physical quantity that groups several units of measure.
/// </summary>
public interface IUnit : IDisposable
{
    /// <summary>
    /// Gets the display name of the quantity.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the quantity description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the unique quantity identifier.
    /// </summary>
    string UnitId { get; }

    /// <summary>
    /// Gets the available unit items keyed by <see cref="IUnitItem.UnitItemId"/>.
    /// </summary>
    IReadOnlyDictionary<string, IUnitItem> AvailableUnits { get; }

    /// <summary>
    /// Gets the user's currently selected and persisted unit item.
    /// </summary>
    BindableReactiveProperty<IUnitItem> CurrentUnitItem { get; }

    /// <summary>
    /// Gets the unit item used to represent this quantity in SI.
    /// </summary>
    IUnitItem InternationalSystemUnit { get; }

    /// <summary>
    /// Gets the icon associated with the quantity.
    /// </summary>
    MaterialIconKind Icon { get; }

    /// <summary>
    /// Gets a unit item by identifier, falling back to <see cref="InternationalSystemUnit"/>.
    /// </summary>
    /// <param name="unitItemId">The unit item identifier.</param>
    /// <returns>The requested unit item, or the SI unit when the identifier is not registered.</returns>
    IUnitItem this[string unitItemId] =>
        AvailableUnits.GetValueOrDefault(unitItemId) ?? InternationalSystemUnit;
}
