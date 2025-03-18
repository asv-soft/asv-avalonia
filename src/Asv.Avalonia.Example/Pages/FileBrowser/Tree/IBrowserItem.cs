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
    BindableReactiveProperty<bool> IsExpanded { get; }
    BindableReactiveProperty<bool> IsSelected { get; }
    BindableReactiveProperty<bool> IsInEditMode { get; }
    string EditedName { get; set; }
    string? Crc32Hex { get; set; }
    SolidColorBrush Crc32Color { get; set; }
    FtpEntryType FtpEntryType { get; set; }
}

public readonly struct FileSize(long size)
{
    private long Size => size;

    public override string ToString()
    {
        string[] sizes =
        [
            RS.Unit_Byte_Abbreviation,
            RS.Unit_Kilobyte_Abbreviation,
            RS.Unit_Megabyte_Abbreviation,
            RS.Unit_Gigabyte_Abbreviation,
            RS.Unit_Terabyte_Abbreviation,
        ];
        double len = size;
        var order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    public int CompareTo(FileSize other)
    {
        return Size.CompareTo(other.Size);
    }
}
