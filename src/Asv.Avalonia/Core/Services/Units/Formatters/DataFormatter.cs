namespace Asv.Avalonia;

public static class DataFormatter
{
    public static IDataFormatter ByteRate { get; } = new ByteRateFormatter();
    public static IDataFormatter DataSize { get; } = new DataSizeFormatter();
}
