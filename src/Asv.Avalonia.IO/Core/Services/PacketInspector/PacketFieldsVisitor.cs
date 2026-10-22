using System.Globalization;
using Asv.IO;

namespace Asv.Avalonia.IO;

internal sealed class PacketFieldsVisitor : FullVisitorBase
{
    private const int MaximumFields = 4096;
    private const int MaximumDepth = 32;
    private readonly Stack<List<PacketFieldSnapshot>> _children = new();
    private readonly Stack<(Field Field, IFieldType Type, string Name)> _parents = new();
    private int _count;

    public PacketFieldsVisitor()
        : base(false) => _children.Push([]);
    public IReadOnlyList<PacketFieldSnapshot> Fields => _children.Peek().ToArray();

    private void Add(Field field, IFieldType type, object? value)
    {
        if (++_count > MaximumFields)
        {
            throw new InvalidOperationException(RS.PacketInspector_FieldLimit);
        }

        var text = value is IFormattable formatted
            ? formatted.ToString(field.GetFormatString(), CultureInfo.InvariantCulture)
            : value?.ToString() ?? "∅";
        if (text.Length > 4096)
        {
            text = text[..4096] + "…";
        }
        _children.Peek().Add(new(ElementName(field), type.Name, text, field.GetUnits(), field.GetDescription(), [])
        {
            Definition = field,
            DataType = type,
            NumericValue = value is Half half ? (double)half : value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double
                ? Convert.ToDouble(value, CultureInfo.InvariantCulture) : null,
        });
    }

    private string ElementName(Field field) => _parents.TryPeek(out var parent)
        && parent.Type is INestedWithSameType ? $"[{_children.Peek().Count}]" : field.Name;

    private void Begin(Field field, IFieldType type)
    {
        if (_parents.Count >= MaximumDepth)
        {
            throw new InvalidOperationException(RS.PacketInspector_FieldLimit);
        }
        var name = ElementName(field);
        _parents.Push((field, type, name));
        _children.Push([]);
    }

    private void End()
    {
        var children = _children.Pop().ToArray();
        var parent = _parents.Pop();
        var field = parent.Field;
        if (++_count > MaximumFields)
        {
            throw new InvalidOperationException(RS.PacketInspector_FieldLimit);
        }
        _children.Peek().Add(new(parent.Name, parent.Type.Name, string.Empty,
            field.GetUnits(), field.GetDescription(), children)
        {
            Definition = field,
            DataType = parent.Type,
        });
    }

    public override void VisitUnknown(Field field, IFieldType type) => Add(field, type, "?");
    public override void BeginList(Field field, ListType type, ref uint size) => Begin(field, type);
    public override void EndList() => End();
    public override void BeginArray(Field field, ArrayType fieldType) => Begin(field, fieldType);
    public override void EndArray() => End();
    public override void BeginStruct(Field field, StructType type) => Begin(field, type);
    public override void EndStruct() => End();
    public override void BeginOptionalStruct(
        Field field,
        OptionalStructType type,
        bool isPresent,
        out bool createNew
    )
    {
        createNew = false;
        Begin(field, type);
    }
    public override void EndOptionalStruct(bool isPresent) => End();
    public override void Visit(Field field, DoubleOptionalType type, ref double? value) => Add(field, type, value);
    public override void Visit(Field field, FloatOptionalType type, ref float? value) => Add(field, type, value);
    public override void Visit(Field field, HalfFloatOptionalType type, ref Half? value) => Add(field, type, value);
    public override void Visit(Field field, Int8OptionalType type, ref sbyte? value) => Add(field, type, value);
    public override void Visit(Field field, Int16OptionalType type, ref short? value) => Add(field, type, value);
    public override void Visit(Field field, Int32OptionalType type, ref int? value) => Add(field, type, value);
    public override void Visit(Field field, Int64OptionalType type, ref long? value) => Add(field, type, value);
    public override void Visit(Field field, UInt8OptionalType type, ref byte? value) => Add(field, type, value);
    public override void Visit(Field field, UInt16OptionalType type, ref ushort? value) => Add(field, type, value);
    public override void Visit(Field field, UInt32OptionalType type, ref uint? value) => Add(field, type, value);
    public override void Visit(Field field, UInt64OptionalType type, ref ulong? value) => Add(field, type, value);
    public override void Visit(Field field, StringOptionalType type, ref string? value) => Add(field, type, value);
    public override void Visit(Field field, BoolOptionalType type, ref bool? value) => Add(field, type, value);
    public override void Visit(Field field, CharOptionalType type, ref char? value) => Add(field, type, value);
    public override void Visit(Field field, DateTimeType type, ref DateTime value) => Add(field, type, value);
    public override void Visit(Field field, DateTimeOptionalType type, ref DateTime? value) => Add(field, type, value);
    public override void Visit(Field field, TimeSpanType type, ref TimeSpan value) => Add(field, type, value);
    public override void Visit(Field field, TimeSpanOptionalType type, ref TimeSpan? value) => Add(field, type, value);
    public override void Visit(Field field, DateOnlyType type, ref DateOnly value) => Add(field, type, value);
    public override void Visit(Field field, DateOnlyOptionalType type, ref DateOnly? value) => Add(field, type, value);
    public override void Visit(Field field, TimeOnlyType type, ref TimeOnly value) => Add(field, type, value);
    public override void Visit(Field field, TimeOnlyOptionalType type, ref TimeOnly? value) => Add(field, type, value);
    public override void Visit(Field field, DoubleType type, ref double value) => Add(field, type, value);
    public override void Visit(Field field, FloatType type, ref float value) => Add(field, type, value);
    public override void Visit(Field field, HalfFloatType type, ref Half value) => Add(field, type, value);
    public override void Visit(Field field, Int8Type type, ref sbyte value) => Add(field, type, value);
    public override void Visit(Field field, Int16Type type, ref short value) => Add(field, type, value);
    public override void Visit(Field field, Int32Type type, ref int value) => Add(field, type, value);
    public override void Visit(Field field, Int64Type type, ref long value) => Add(field, type, value);
    public override void Visit(Field field, UInt8Type type, ref byte value) => Add(field, type, value);
    public override void Visit(Field field, UInt16Type type, ref ushort value) => Add(field, type, value);
    public override void Visit(Field field, UInt32Type type, ref uint value) => Add(field, type, value);
    public override void Visit(Field field, UInt64Type type, ref ulong value) => Add(field, type, value);
    public override void Visit(Field field, StringType type, ref string value) => Add(field, type, value);
    public override void Visit(Field field, BoolType type, ref bool value) => Add(field, type, value);
    public override void Visit(Field field, CharType type, ref char value) => Add(field, type, value);
}
