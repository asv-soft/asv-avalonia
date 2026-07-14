namespace Asv.Avalonia;

public class DataSizeFormatter : IDataFormatter
{
    private readonly ScaledUnitValueFormatter _formatter;

    public DataSizeFormatter(IUnitService unitService)
    {
        _formatter = ScaledUnitValueFormatter.Create(unitService, DataSizeUnit.Id);
    }

    public const string StaticId = "byte_size";
    public string Name => "Byte size";
    public string Description => "Format value as byte size (bytes 1024-based)";
    public string Id => StaticId;

    public string Print(double value, string? format = null)
    {
        return _formatter.Print(value, format);
    }
}
