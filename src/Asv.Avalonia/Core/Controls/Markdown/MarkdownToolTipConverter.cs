using System.Globalization;
using System.Text;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Asv.Avalonia;

public class MarkdownToolTipConverter : IValueConverter, IMultiValueConverter
{
    public static MarkdownToolTipConverter Instance { get; } = new();

    public double MaxWidth { get; set; } = 360;

    public TextWrapping TextWrapping { get; set; } = TextWrapping.Wrap;

    public double BlockSpacing { get; set; } = 6;

    public double ListItemSpacing { get; set; } = 2;

    public double ListIndent { get; set; } = 12;

    public double IconSize { get; set; } = 14;

    public int HeaderLevel { get; set; } = 3;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return CreateToolTip(GetString(parameter), GetString(value));
    }

    public object? Convert(
        IList<object?> values,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        var header = values.Count > 1 ? GetString(values[0]) : null;
        var description = values.Count > 1 ? GetString(values[1]) : GetString(values, 0);
        return CreateToolTip(header, description);
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        throw new NotImplementedException();
    }

    private object? CreateToolTip(string? header, string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        return new MarkdownViewer
        {
            MaxWidth = MaxWidth,
            TextWrapping = TextWrapping,
            BlockSpacing = BlockSpacing,
            ListItemSpacing = ListItemSpacing,
            ListIndent = ListIndent,
            IconSize = IconSize,
            Text = CreateMarkdownText(header, description),
        };
    }

    private string CreateMarkdownText(string? header, string description)
    {
        var trimmedDescription = description.Trim();
        if (string.IsNullOrWhiteSpace(header))
        {
            return trimmedDescription;
        }

        var headerLevel = Math.Clamp(HeaderLevel, 1, 3);
        var headerPrefix = new string('#', headerLevel);
        return $"{headerPrefix} {EscapeInlineMarkdown(header.Trim())}{Environment.NewLine}{Environment.NewLine}{trimmedDescription}";
    }

    private static string? GetString(IList<object?> values, int index)
    {
        if (values.Count <= index)
        {
            return null;
        }

        return GetString(values[index]);
    }

    private static string? GetString(object? value)
    {
        if (value is null || value == BindingOperations.DoNothing)
        {
            return null;
        }

        return value as string ?? value.ToString();
    }

    private static string EscapeInlineMarkdown(string value)
    {
        var result = new StringBuilder(value.Length);
        foreach (var item in value.ReplaceLineEndings(" "))
        {
            if (item is '[' or ']' or '\\')
            {
                result.Append('\\');
            }

            result.Append(item);
        }

        return result.ToString();
    }
}
