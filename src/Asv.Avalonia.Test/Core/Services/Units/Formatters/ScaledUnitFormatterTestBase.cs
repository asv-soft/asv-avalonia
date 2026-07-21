using System.Globalization;
using Xunit;

namespace Asv.Avalonia.Test;

public abstract class ScaledUnitFormatterTestBase : IDisposable
{
    private readonly IUnit _unit;
    private readonly double _scaleFactor;
    private readonly string[] _unitItemIds;

    protected ScaledUnitFormatterTestBase(
        IUnit unit,
        double scaleFactor,
        IEnumerable<string> unitItemIds
    )
    {
        _unit = unit;
        _scaleFactor = scaleFactor;
        _unitItemIds = unitItemIds.ToArray();
    }

    protected abstract IDataFormatter CreateFormatter(IUnitService unitService);

    [Theory]
    [InlineData(0, 1.0)]
    [InlineData(1, 1.0)]
    [InlineData(2, 2.0)]
    [InlineData(3, 3.0)]
    [InlineData(4, 4.0)]
    public void Print_AvailableItems_SelectsBestUnit(int exponent, double multiplier)
    {
        // Arrange
        var unitService = new UnitService([_unit]);
        var formatter = CreateFormatter(unitService);
        var value =
            exponent == 0 ? _scaleFactor - 1 : multiplier * Math.Pow(_scaleFactor, exponent);
        var expectedValue = (exponent == 0 ? value : multiplier).ToString(
            "0.##",
            CultureInfo.InvariantCulture
        );
        var expectedUnit = _unit.AvailableUnits[_unitItemIds[exponent]];
        var expected = $"{expectedValue} {expectedUnit.Symbol}";

        // Act
        var actual = formatter.Print(value, "0.##");

        // Assert
        Assert.Equal(expected, actual);
    }

    public void Dispose()
    {
        _unit.Dispose();
    }
}
