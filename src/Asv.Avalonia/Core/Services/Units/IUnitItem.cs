using Asv.Common;

namespace Asv.Avalonia;

/// <summary>
/// Represents a concrete unit of measure that handles conversion, parsing, validation and formatting.
/// </summary>
public interface IUnitItem
{
    /// <summary>
    /// Gets the unique unit item identifier.
    /// </summary>
    string UnitItemId { get; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the unit description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the symbol appended to formatted values.
    /// </summary>
    string Symbol { get; }

    /// <summary>
    /// Gets a value indicating whether this item represents the quantity in SI.
    /// </summary>
    bool IsInternationalSystemUnit { get; }

    /// <summary>
    /// Determines whether text is a valid value in this unit.
    /// </summary>
    /// <param name="value">The text to validate.</param>
    /// <returns><see langword="true"/> when the value is valid; otherwise, <see langword="false"/>.</returns>
    bool IsValid(string? value);

    /// <summary>
    /// Validates text as a value in this unit.
    /// </summary>
    /// <param name="value">The text to validate.</param>
    /// <returns>The validation result.</returns>
    ValidationResult ValidateValue(string? value);

    /// <summary>
    /// Parses text as a value in this unit.
    /// </summary>
    /// <param name="value">The text to parse.</param>
    /// <returns>The parsed value expressed in this unit.</returns>
    double Parse(string? value);

    /// <summary>
    /// Parses text as a value in this unit and converts it to SI.
    /// </summary>
    /// <param name="value">The text to parse.</param>
    /// <returns>The parsed value expressed in SI.</returns>
    double ParseToSi(string? value) => ToSi(Parse(value));

    /// <summary>
    /// Formats a value expressed in this unit.
    /// </summary>
    /// <param name="value">The value expressed in this unit.</param>
    /// <param name="format">The numeric format string, or <see langword="null"/> for the default format.</param>
    /// <returns>The formatted value.</returns>
    string Print(double value, string? format = null);

    /// <summary>
    /// Converts an SI value to this unit and formats it.
    /// </summary>
    /// <param name="value">The value expressed in SI.</param>
    /// <param name="format">The numeric format string, or <see langword="null"/> for the default format.</param>
    /// <returns>The converted and formatted value.</returns>
    string PrintFromSi(double value, string? format = null) => Print(FromSi(value), format);

    /// <summary>
    /// Formats a value expressed in this unit and appends <see cref="Symbol"/>.
    /// </summary>
    /// <param name="value">The value expressed in this unit.</param>
    /// <param name="format">The numeric format string, or <see langword="null"/> for the default format.</param>
    /// <returns>The formatted value and unit symbol.</returns>
    string PrintWithUnits(double value, string? format = null);

    /// <summary>
    /// Converts an SI value to this unit, formats it and appends <see cref="Symbol"/>.
    /// </summary>
    /// <param name="value">The value expressed in SI.</param>
    /// <param name="format">The numeric format string, or <see langword="null"/> for the default format.</param>
    /// <returns>The converted and formatted value with its unit symbol.</returns>
    string PrintFromSiWithUnits(double value, string? format = null) =>
        PrintWithUnits(FromSi(value), format);

    /// <summary>
    /// Converts a value from SI to this unit.
    /// </summary>
    /// <param name="siValue">The value expressed in SI.</param>
    /// <returns>The value expressed in this unit.</returns>
    double FromSi(double siValue);

    /// <summary>
    /// Converts a value from this unit to SI.
    /// </summary>
    /// <param name="value">The value expressed in this unit.</param>
    /// <returns>The value expressed in SI.</returns>
    double ToSi(double value);
}
