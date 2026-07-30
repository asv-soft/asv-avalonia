using Asv.Cfg;
using Xunit;

namespace Asv.Avalonia.Test;

public sealed class DistanceNauticalMileUnitItemTest()
    : UnitItemTestBase<UnitItemDefaultTestCases>(
        DistanceUnitTestSetup.CreateUnit(),
        DistanceNauticalMileUnitItem.Id,
        1852.0
    ) { }

internal static class DistanceUnitTestSetup
{
    public static DistanceUnit CreateUnit()
    {
        IUnitItem[] items = [new DistanceMeterUnitItem(), new DistanceNauticalMileUnitItem()];

        return new DistanceUnit(new InMemoryConfiguration(), items);
    }
}
