namespace Asv.Avalonia;

public class ByteRateFormatter : IDataFormatter
{
    private readonly ScaledUnitValueFormatter _formatter;

    public ByteRateFormatter(IUnitService unitService)
    {
        _formatter = ScaledUnitValueFormatter.Create(unitService, DataRateUnit.Id);
    }

    public const string StaticId = "byte_rate";
    public string Name => "Byte rate";
    public string Description => "Format value as byte rate (bytes per second)";
    public string Id => StaticId;

    public string Print(double value, string? format = null)
    {
        return _formatter.Print(value, format);
    }
}
