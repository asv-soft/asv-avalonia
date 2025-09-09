using System.Globalization;
using Avalonia.Data.Converters;

namespace Asv.Avalonia;

public enum CommandSortingType
{
    All,
    WithHotkeysOnly,
    WithoutHotkeysOnly,
}

public class CommandsSortingTypeLocalizationConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }

        return (value as CommandSortingType?) switch
        {
            CommandSortingType.All => RS.SettingsCommandListView_CommandsSortingType_All,
            CommandSortingType.WithHotkeysOnly =>
                RS.SettingsCommandListView_CommandsSortingType_WithHotkeysOnly,
            CommandSortingType.WithoutHotkeysOnly =>
                RS.SettingsCommandListView_CommandsSortingType_WithoutHotkeysOnly,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        if (value is null)
        {
            return null;
        }

        var commandSortingTypeRaw = value as string;

        if (commandSortingTypeRaw == RS.SettingsCommandListView_CommandsSortingType_All)
        {
            return CommandSortingType.All;
        }
        if (commandSortingTypeRaw == RS.SettingsCommandListView_CommandsSortingType_WithHotkeysOnly)
        {
            return CommandSortingType.WithHotkeysOnly;
        }
        if (
            commandSortingTypeRaw
            == RS.SettingsCommandListView_CommandsSortingType_WithoutHotkeysOnly
        )
        {
            return CommandSortingType.WithoutHotkeysOnly;
        }

        throw new ArgumentOutOfRangeException(nameof(value), value, null);
    }
}
