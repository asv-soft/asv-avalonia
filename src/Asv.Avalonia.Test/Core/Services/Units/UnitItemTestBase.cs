using Asv.Common;
using Xunit;

namespace Asv.Avalonia.Test;

public abstract class UnitItemTestBase<TTestCases> : IDisposable
    where TTestCases : IUnitItemTestCases
{
    private const string Format = "0.###";
    private readonly IUnit _unit;
    private readonly IUnitItem _item;
    private readonly double _siValuePerUnit;

    protected UnitItemTestBase(IUnit unit, string unitItemId, double siValuePerUnit)
    {
        _unit = unit;
        _item = unit.AvailableUnits[unitItemId];
        _siValuePerUnit = siValuePerUnit;
    }

    public static TheoryData<string> ValidTextCases => TTestCases.ValidTextCases;
    public static TheoryData<string> InvalidTextCases => TTestCases.InvalidTextCases;
    public static TheoryData<string, double> ParseCases => TTestCases.ParseCases;
    public static TheoryData<double, string> PrintCases => TTestCases.PrintCases;
    public static TheoryData<double> NumericCases => TTestCases.NumericCases;

    [Theory]
    [MemberData(nameof(ValidTextCases))]
    public void IsValid_ValidValue_ReturnsTrue(string value)
    {
        // Act
        var actual = _item.IsValid(value);

        // Assert
        Assert.True(actual);
    }

    [Theory]
    [MemberData(nameof(InvalidTextCases))]
    public void IsValid_InvalidValue_ReturnsFalse(string value)
    {
        // Act
        var actual = _item.IsValid(value);

        // Assert
        Assert.False(actual);
    }

    [Theory]
    [MemberData(nameof(ValidTextCases))]
    public void ValidateValue_ValidValue_ReturnsSuccess(string value)
    {
        // Act
        var actual = _item.ValidateValue(value);

        // Assert
        Assert.True(actual.IsSuccess);
    }

    [Theory]
    [MemberData(nameof(InvalidTextCases))]
    public void ValidateValue_InvalidValue_ReturnsFailure(string value)
    {
        // Act
        var actual = _item.ValidateValue(value);

        // Assert
        Assert.False(actual.IsSuccess);
    }

    [Theory]
    [MemberData(nameof(ParseCases))]
    public void Parse_ValidValue_ReturnsValue(string value, double expected)
    {
        // Act
        var actual = _item.Parse(value);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(ParseCases))]
    public void ParseToSi_ValidValue_ReturnsBytes(string value, double parsedValue)
    {
        // Arrange
        var expected = parsedValue * _siValuePerUnit;

        // Act
        var actual = _item.ParseToSi(value);

        // Assert
        Assert.True(expected.ApproximatelyEquals(actual, GetEpsilon(expected)));
    }

    [Theory]
    [MemberData(nameof(PrintCases))]
    public void Print_ValidValue_ReturnsFormattedValue(double value, string expected)
    {
        // Act
        var actual = _item.Print(value, Format);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(PrintCases))]
    public void PrintFromSi_Bytes_ReturnsFormattedValue(double value, string expected)
    {
        // Arrange
        var siValue = value * _siValuePerUnit;

        // Act
        var actual = _item.PrintFromSi(siValue, Format);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(PrintCases))]
    public void PrintWithUnits_ValidValue_ReturnsFormattedValueWithSymbol(
        double value,
        string printedValue
    )
    {
        // Arrange
        var expected = $"{printedValue} {_item.Symbol}";

        // Act
        var actual = _item.PrintWithUnits(value, Format);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(PrintCases))]
    public void PrintFromSiWithUnits_Bytes_ReturnsFormattedValueWithSymbol(
        double value,
        string printedValue
    )
    {
        // Arrange
        var siValue = value * _siValuePerUnit;
        var expected = $"{printedValue} {_item.Symbol}";

        // Act
        var actual = _item.PrintFromSiWithUnits(siValue, Format);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(NumericCases))]
    public void FromSi_Bytes_ReturnsValue(double expected)
    {
        // Arrange
        var siValue = expected * _siValuePerUnit;

        // Act
        var actual = _item.FromSi(siValue);

        // Assert
        Assert.True(expected.ApproximatelyEquals(actual, GetEpsilon(expected)));
    }

    [Theory]
    [MemberData(nameof(NumericCases))]
    public void ToSi_ValidValue_ReturnsBytes(double value)
    {
        // Arrange
        var expected = value * _siValuePerUnit;

        // Act
        var actual = _item.ToSi(value);

        // Assert
        Assert.True(expected.ApproximatelyEquals(actual, GetEpsilon(expected)));
    }

    private static double GetEpsilon(double expected)
    {
        return Math.Max(FloatingPointComparer.Epsilon, Math.Abs(expected) * 1e-12);
    }

    public void Dispose()
    {
        _unit.Dispose();
    }
}

public interface IUnitItemTestCases
{
    static abstract TheoryData<string> ValidTextCases { get; }
    static abstract TheoryData<string> InvalidTextCases { get; }
    static abstract TheoryData<string, double> ParseCases { get; }
    static abstract TheoryData<double, string> PrintCases { get; }
    static abstract TheoryData<double> NumericCases { get; }
}

public sealed class UnitItemDefaultTestCases : IUnitItemTestCases
{
    public static TheoryData<string> ValidTextCases => ["0", "0.5", "1", "1.5", "10.25"];

    public static TheoryData<string> InvalidTextCases => ["invalid"];

    public static TheoryData<string, double> ParseCases =>
        new()
        {
            { "0", 0.0 },
            { "0.5", 0.5 },
            { "1", 1.0 },
            { "1.5", 1.5 },
            { "10.25", 10.25 },
        };

    public static TheoryData<double, string> PrintCases =>
        new()
        {
            { 0.0, "0" },
            { 0.5, "0.5" },
            { 1.0, "1" },
            { 1.5, "1.5" },
            { 10.25, "10.25" },
        };

    public static TheoryData<double> NumericCases => [0.0, 0.5, 1.0, 1.5, 10.25];
}
