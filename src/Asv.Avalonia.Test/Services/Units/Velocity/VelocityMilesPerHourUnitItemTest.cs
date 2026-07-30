using Asv.Cfg;

namespace Asv.Avalonia.Test;

public sealed class VelocityMilesPerHourUnitItemTest()
    : UnitItemTestBase<UnitItemDefaultTestCases>(
        VelocityUnitTestSetup.CreateUnit(),
        VelocityMilesPerHourUnitItem.Id,
        0.44704
    ) { }

internal static class VelocityUnitTestSetup
{
    public static VelocityUnit CreateUnit()
    {
        IUnitItem[] items =
        [
            new VelocityMetersPerSecondUnitItem(),
            new VelocityKilometersPerHourUnitItem(),
            new VelocityMilesPerHourUnitItem(),
        ];

        return new VelocityUnit(new InMemoryConfiguration(), items);
    }
}
