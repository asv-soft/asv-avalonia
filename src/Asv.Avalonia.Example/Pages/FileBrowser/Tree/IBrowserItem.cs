using Asv.Mavlink;
using Avalonia.Media;
using R3;

namespace Asv.Avalonia.Example;

public interface IBrowserItem : IHeadlinedViewModel
{
    string Path { get; }
    NavigationId ParentId { get; }
    FileSize? Size { get; }
    bool HasChildren { get; }
    bool IsExpanded { get; }
    bool IsSelected { get; }
    bool IsInEditMode { get; }
    string EditedName { get; }
    string? Crc32Hex { get; }
    SolidColorBrush Crc32Color { get; }
    FtpEntryType FtpEntryType { get; }
}

public readonly struct FileSize(long size)
{
    private long Size => size;

    public override string ToString()
    {
        string unit;
        double value;

        switch (size)
        {
            case < 1024:
                unit = RS.Unit_Byte_Abbreviation;
                value = size;
                break;
            case < 1024L * 1024:
                unit = RS.Unit_Kilobyte_Abbreviation;
                value = size / 1024d;
                break;
            case < 1024L * 1024 * 1024:
                unit = RS.Unit_Megabyte_Abbreviation;
                value = size / (1024d * 1024);
                break;
            case < 1024L * 1024 * 1024 * 1024:
                unit = RS.Unit_Gigabyte_Abbreviation;
                value = size / (1024d * 1024 * 1024);
                break;
            default:
                unit = RS.Unit_Terabyte_Abbreviation;
                value = size / (1024d * 1024 * 1024 * 1024);
                break;
        }

        return $"{value:0.##} {unit}";
    }

    public int CompareTo(FileSize other)
    {
        return Size.CompareTo(other.Size);
    }
}
