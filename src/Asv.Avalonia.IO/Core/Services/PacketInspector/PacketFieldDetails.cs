using System.Collections;
using System.Globalization;
using System.Text;
using Asv.IO;

namespace Asv.Avalonia.IO;

public sealed record PacketFieldInfoRequest(PacketFieldSnapshot field, string path)
{
    public PacketFieldSnapshot Field { get; } = field;
    public string Path { get; } = path;
}

internal static class PacketFieldDetails
{
    public static string CreateMarkdown(PacketFieldSnapshot snapshot, string path)
    {
        var field = snapshot.Definition;
        var builder = new StringBuilder();
        builder.Append("# ").AppendLine(Escape(field?.GetTitle() ?? snapshot.Name)).AppendLine();
        if (!string.IsNullOrWhiteSpace(snapshot.Description))
        {
            builder.AppendLine(snapshot.Description).AppendLine();
        }
        Item(RS.PacketInspector_FieldPath, path);
        Item(RS.PacketInspector_FieldType, snapshot.Type);
        if (!string.IsNullOrEmpty(snapshot.Value))
        {
            Item(RS.PacketInspector_FieldValue, snapshot.Value);
        }
        if (!string.IsNullOrWhiteSpace(snapshot.Units))
        {
            Item(RS.PacketInspector_FieldUnits, snapshot.Units);
        }
        if (snapshot.Children.Count > 0)
        {
            Item(RS.PacketInspector_FieldElements, snapshot.Children.Count);
        }

        switch (snapshot.DataType)
        {
            case ListType list:
                Item(RS.PacketInspector_FieldElementType, list.ElementType.Name);
                Item(RS.PacketInspector_FieldMinimumSize, list.MinSize);
                Item(RS.PacketInspector_FieldMaximumSize, list.MaxSize);
                break;
            case ArrayType array:
                Item(RS.PacketInspector_FieldElementType, array.ElementType.Name);
                Item(RS.PacketInspector_FieldSize, array.Size);
                break;
            case StringType text:
                TextDetails(text.Encoding.ToString(), text.MinSize, text.MaxSize, text.AllowedChars);
                break;
            case StringOptionalType text:
                TextDetails(text.Encoding.ToString(), text.MinSize, text.MaxSize, text.AllowedChars);
                break;
        }
        AppendNumberDetails(snapshot.DataType, Item);
        if (field != null)
        {
            if (field.GetFormatString() is { } format)
            {
                Item(RS.PacketInspector_FieldFormat, format);
            }
            foreach (var pair in field.Metadata.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                switch (pair.Key)
                {
                    case "title" or "description" or "units" or "format":
                        break;
                    case "readOnly" when pair.Value is bool readOnly:
                        Item(RS.PacketInspector_FieldReadOnly, readOnly ? RS.PacketInspector_FieldYes : RS.PacketInspector_FieldNo);
                        break;
                    case "enum" when pair.Value is IDictionary values:
                        builder.AppendLine().Append("## ").AppendLine(RS.PacketInspector_FieldEnum).AppendLine();
                        foreach (var key in values.Keys.Cast<object>().OrderBy(Format, StringComparer.Ordinal))
                        {
                            builder.Append("- ").AppendLine(Escape(FormatEnum(values[key])));
                        }
                        break;
                    default:
                        if (pair.Value != null)
                        {
                            Item(pair.Key, FormatEnum(pair.Value));
                        }
                        break;
                }
            }
        }
        return builder.ToString();

        void Item(string label, object? value) => builder.Append("- **").Append(Escape(label))
            .Append(":** ").AppendLine(Escape(Format(value)));

        void TextDetails(string encoding, uint min, uint max, string? allowed)
        {
            Item(RS.PacketInspector_FieldEncoding, encoding);
            Item(RS.PacketInspector_FieldMinimumSize, min);
            Item(RS.PacketInspector_FieldMaximumSize, max);
            if (allowed != null)
            {
                Item(RS.PacketInspector_FieldAllowedCharacters, allowed);
            }
        }
    }

    private static string Format(object? value) => value is IFormattable formatted
        ? formatted.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty
        : value?.ToString() ?? "∅";

    private static string Escape(string value) => value.Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("[", "\\[", StringComparison.Ordinal).Replace("]", "\\]", StringComparison.Ordinal)
        .Replace("*", "\\*", StringComparison.Ordinal).Replace("\r", string.Empty, StringComparison.Ordinal)
        .Replace("\n", " ", StringComparison.Ordinal);

    private static string EnumDetails<T>(EnumValue<T> value) =>
        Format(value.Value) + " — " + value.Name
        + (string.IsNullOrWhiteSpace(value.Description) ? string.Empty : ": " + value.Description);

    private static string FormatEnum(object? value) => value switch
    {
        EnumValue<byte> item => EnumDetails(item),
        EnumValue<sbyte> item => EnumDetails(item),
        EnumValue<short> item => EnumDetails(item),
        EnumValue<ushort> item => EnumDetails(item),
        EnumValue<int> item => EnumDetails(item),
        EnumValue<uint> item => EnumDetails(item),
        EnumValue<long> item => EnumDetails(item),
        EnumValue<ulong> item => EnumDetails(item),
        EnumValue<float> item => EnumDetails(item),
        EnumValue<double> item => EnumDetails(item),
        EnumValue<bool> item => EnumDetails(item),
        EnumValue<string> item => EnumDetails(item),
        _ => Format(value),
    };

    private static void AppendNumberDetails(IFieldType? type, Action<string, object?> item)
    {
        switch (type)
        {
            case UInt8Type number:
                item(RS.PacketInspector_FieldMinimum, number.Min);
                item(RS.PacketInspector_FieldMaximum, number.Max);
                item(RS.PacketInspector_FieldDefault, number.DefaultValue);
                break;
            case UInt8OptionalType number:
                item(RS.PacketInspector_FieldMinimum, number.Min);
                item(RS.PacketInspector_FieldMaximum, number.Max);
                item(RS.PacketInspector_FieldDefault, number.DefaultValue);
                break;
            case Int8Type number:
                item(RS.PacketInspector_FieldMinimum, number.Min);
                item(RS.PacketInspector_FieldMaximum, number.Max);
                item(RS.PacketInspector_FieldDefault, number.DefaultValue);
                break;
            case Int8OptionalType number:
                item(RS.PacketInspector_FieldMinimum, number.Min);
                item(RS.PacketInspector_FieldMaximum, number.Max);
                item(RS.PacketInspector_FieldDefault, number.DefaultValue);
                break;
            case UInt16Type number:
                item(RS.PacketInspector_FieldMinimum, number.Min);
                item(RS.PacketInspector_FieldMaximum, number.Max);
                item(RS.PacketInspector_FieldDefault, number.DefaultValue);
                break;
            case UInt16OptionalType number:
                item(RS.PacketInspector_FieldMinimum, number.Min);
                item(RS.PacketInspector_FieldMaximum, number.Max);
                item(RS.PacketInspector_FieldDefault, number.DefaultValue);
                break;
            case Int16Type number:
                item(RS.PacketInspector_FieldMinimum, number.Min);
                item(RS.PacketInspector_FieldMaximum, number.Max);
                item(RS.PacketInspector_FieldDefault, number.DefaultValue);
                break;
            case Int16OptionalType number:
                item(RS.PacketInspector_FieldMinimum, number.Min);
                item(RS.PacketInspector_FieldMaximum, number.Max);
                item(RS.PacketInspector_FieldDefault, number.DefaultValue);
                break;
            case UInt32Type number:
                item(RS.PacketInspector_FieldMinimum, number.Min);
                item(RS.PacketInspector_FieldMaximum, number.Max);
                item(RS.PacketInspector_FieldDefault, number.DefaultValue);
                break;
            case UInt32OptionalType number:
                item(RS.PacketInspector_FieldMinimum, number.Min);
                item(RS.PacketInspector_FieldMaximum, number.Max);
                item(RS.PacketInspector_FieldDefault, number.DefaultValue);
                break;
            case Int32Type number:
                item(RS.PacketInspector_FieldMinimum, number.Min);
                item(RS.PacketInspector_FieldMaximum, number.Max);
                item(RS.PacketInspector_FieldDefault, number.DefaultValue);
                break;
            case Int32OptionalType number:
                item(RS.PacketInspector_FieldMinimum, number.Min);
                item(RS.PacketInspector_FieldMaximum, number.Max);
                item(RS.PacketInspector_FieldDefault, number.DefaultValue);
                break;
            case UInt64Type number:
                item(RS.PacketInspector_FieldMinimum, number.Min);
                item(RS.PacketInspector_FieldMaximum, number.Max);
                item(RS.PacketInspector_FieldDefault, number.DefaultValue);
                break;
            case UInt64OptionalType number:
                item(RS.PacketInspector_FieldMinimum, number.Min);
                item(RS.PacketInspector_FieldMaximum, number.Max);
                item(RS.PacketInspector_FieldDefault, number.DefaultValue);
                break;
            case Int64Type number:
                item(RS.PacketInspector_FieldMinimum, number.Min);
                item(RS.PacketInspector_FieldMaximum, number.Max);
                item(RS.PacketInspector_FieldDefault, number.DefaultValue);
                break;
            case Int64OptionalType number:
                item(RS.PacketInspector_FieldMinimum, number.Min);
                item(RS.PacketInspector_FieldMaximum, number.Max);
                item(RS.PacketInspector_FieldDefault, number.DefaultValue);
                break;
            case HalfFloatType number:
                item(RS.PacketInspector_FieldMinimum, number.Min);
                item(RS.PacketInspector_FieldMaximum, number.Max);
                item(RS.PacketInspector_FieldDefault, number.DefaultValue);
                break;
            case HalfFloatOptionalType number:
                item(RS.PacketInspector_FieldMinimum, number.Min);
                item(RS.PacketInspector_FieldMaximum, number.Max);
                item(RS.PacketInspector_FieldDefault, number.DefaultValue);
                break;
            case FloatType number:
                item(RS.PacketInspector_FieldMinimum, number.Min);
                item(RS.PacketInspector_FieldMaximum, number.Max);
                item(RS.PacketInspector_FieldDefault, number.DefaultValue);
                break;
            case FloatOptionalType number:
                item(RS.PacketInspector_FieldMinimum, number.Min);
                item(RS.PacketInspector_FieldMaximum, number.Max);
                item(RS.PacketInspector_FieldDefault, number.DefaultValue);
                break;
            case DoubleType number:
                item(RS.PacketInspector_FieldMinimum, number.Min);
                item(RS.PacketInspector_FieldMaximum, number.Max);
                item(RS.PacketInspector_FieldDefault, number.DefaultValue);
                break;
            case DoubleOptionalType number:
                item(RS.PacketInspector_FieldMinimum, number.Min);
                item(RS.PacketInspector_FieldMaximum, number.Max);
                item(RS.PacketInspector_FieldDefault, number.DefaultValue);
                break;
        }
    }
}
