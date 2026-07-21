using Xunit;

namespace Asv.Avalonia.Test;

public class UnitItemBaseTest
{
    [Fact]
    public void PrintFromSi_Format_AppliesAfterSiConversion()
    {
        var unit = new VelocityKilometersPerHourUnitItem();

        var actual = unit.PrintFromSi(1, "0.00");

        Assert.Equal("3.60", actual);
    }
}
