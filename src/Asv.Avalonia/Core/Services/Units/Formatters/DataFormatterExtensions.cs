namespace Asv.Avalonia;

public static class DataFormatterExtensions
{
    public static IDataFormatter CreateByteRateFormatter(this IUnitService unitService)
    {
        ArgumentNullException.ThrowIfNull(unitService);
        return new ByteRateFormatter(unitService);
    }

    public static IDataFormatter CreateDataSizeFormatter(this IUnitService unitService)
    {
        ArgumentNullException.ThrowIfNull(unitService);
        return new DataSizeFormatter(unitService);
    }
}
