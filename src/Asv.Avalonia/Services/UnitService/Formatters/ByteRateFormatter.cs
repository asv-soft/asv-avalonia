namespace Asv.Avalonia;

public class DataSizeFormatter : IDataFormatter
{
    private const double OneKb = 1024.0;
    private const double OneMb = OneKb * OneKb;
    private const double OneGb = OneMb * OneKb;

    public const string StaticId = "byte_size";
    public string Name => "Byte size";
    public string Description => "Format value as byte size (bytes 1024-based)";
    public string Id => StaticId;

    public string Print(double value, string? format = null)
    {
        var unit = value switch
        {
            < 0 => string.Empty,
            <= OneKb => "B",
            >= OneKb and < OneMb => "KB",
            >= OneMb and < OneGb => "MB",
            >= OneGb => "GB",
            _ => Units.NotAvailableString,
        };

        return value switch
        {
            0 => format == null ? $"{value,-4:F0} {unit}" : value.ToString(format) + $" {unit}",
            < 1 => format == null ? $"{value,-4:F3} {unit}" : value.ToString(format) + $" {unit}",
            < OneKb => format == null
                ? $"{value,-4:F0} {unit}"
                : value.ToString(format) + $" {unit}",
            >= OneKb and < OneMb => format == null
                ? $"{value / OneKb,-4:F0} {unit}"
                : (value / OneKb).ToString(format) + $" {unit}",
            >= OneMb and < OneGb => format == null
                ? $"{value / OneMb,-4:F0} {unit}"
                : (value / OneMb).ToString(format) + $" {unit}",
            >= OneGb => format == null
                ? $"{value / OneGb,-4:F0} {unit}"
                : (value / OneGb).ToString(format) + $" {unit}",
            _ => Units.NotAvailableString,
        };
    }
}

public class ByteRateFormatter : IDataFormatter
{
    private const double OneKb = 1024.0;
    private const double OneMb = OneKb * OneKb;
    private const double OneGb = OneMb * OneKb;

    public const string StaticId = "byte_rate";
    public string Name => "Byte rate";
    public string Description => "Format value as byte rate (bytes per second)";
    public string Id => StaticId;

    public string Print(double value, string? format = null)
    {
        var unit = value switch
        {
            < 0 => string.Empty,
            <= OneKb => "B/s",
            >= OneKb and < OneMb => "KB/s",
            >= OneMb and < OneGb => "MB/s",
            >= OneGb => "GB/s",
            _ => Units.NotAvailableString,
        };

        return value switch
        {
            0 => format == null ? $"{value,-4:F0} {unit}" : value.ToString(format) + $" {unit}",
            < 1 => format == null ? $"{value,-4:F3} {unit}" : value.ToString(format) + $" {unit}",
            < OneKb => format == null
                ? $"{value,-4:F0} {unit}"
                : value.ToString(format) + $" {unit}",
            >= OneKb and < OneMb => format == null
                ? $"{value / OneKb,-4:F0} {unit}"
                : (value / OneKb).ToString(format) + $" {unit}",
            >= OneMb and < OneGb => format == null
                ? $"{value / OneMb,-4:F0} {unit}"
                : (value / OneMb).ToString(format) + $" {unit}",
            >= OneGb => format == null
                ? $"{value / OneGb,-4:F0} {unit}"
                : (value / OneGb).ToString(format) + $" {unit}",
            _ => Units.NotAvailableString,
        };
    }
}
