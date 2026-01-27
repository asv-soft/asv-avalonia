using Asv.IO;
using Newtonsoft.Json;

namespace Asv.Avalonia;

public partial class CommandArg
{
    public static CommandArg EmptyString => new StringArg(string.Empty);

    public static StringArg CreateString(string value)
    {
        return new StringArg(value);
    }

    public string AsString()
    {
        if (this is StringArg stringArg)
        {
            return stringArg.Value;
        }

        throw new ArgumentException($"Cannot convert {GetType().Name} to {nameof(StringArg)}.");
    }
}

public class StringArg(string value) : CommandArg
{
    protected internal static CommandArg CreateDefault() => new StringArg(string.Empty);

    public string Value { get; private set; } = value;

    public override Id TypeId => Id.String;

    protected override void InternalDeserialize(ref ReadOnlySpan<byte> buffer) =>
        Value = BinSerialize.ReadString(ref buffer);

    protected override void InternalSerialize(ref Span<byte> buffer) =>
        BinSerialize.WriteString(ref buffer, Value);

    protected override int InternalGetByteSize() => BinSerialize.GetSizeForString(Value);

    protected override void InternalDeserialize(JsonReader reader)
    {
        Value =
            reader.ReadAsString()
            ?? throw new JsonSerializationException("Expected a string value.");
    }

    protected override void InternalSerialize(JsonWriter writer)
    {
        writer.WriteValue(Value);
    }

    public override string ToString() => $"'{Value}'";
}
