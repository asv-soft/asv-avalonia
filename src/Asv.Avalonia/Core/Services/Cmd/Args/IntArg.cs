using Asv.IO;
using Newtonsoft.Json;

namespace Asv.Avalonia;

public partial class CommandArg
{
    public static CommandArg Integer(int value)
    {
        return new IntArg(value);
    }

    public static CommandArg Integer(long value)
    {
        return new IntArg(value);
    }

    public static explicit operator int(CommandArg value)
    {
        if (value is IntArg arg)
        {
            return (int)arg.Value;
        }

        throw new InvalidCastException($"Cannot cast {value.GetType().Name} to {nameof(Int32)}");
    }

    public static implicit operator CommandArg(int value)
    {
        return new IntArg(value);
    }

    public static IntArg CreateInteger(int value)
    {
        return new IntArg(value);
    }

    public static IntArg CreateInteger(long value)
    {
        return new IntArg(value);
    }

    public int AsInt()
    {
        if (this is IntArg arg)
        {
            return (int)arg.Value;
        }

        throw new ArgumentException($"Cannot cast {this.GetType().Name} to {nameof(IntArg)}");
    }
}

public class IntArg(long value) : CommandArg
{
    protected internal static CommandArg CreateDefault() => new IntArg(0);

    private long _value = value;
    public long Value => _value;

    public override Id TypeId => Id.Integer;

    protected override void InternalDeserialize(ref ReadOnlySpan<byte> buffer) =>
        BinSerialize.ReadLong(ref buffer, ref _value);

    protected override void InternalSerialize(ref Span<byte> buffer) =>
        BinSerialize.WriteLong(ref buffer, _value);

    protected override int InternalGetByteSize() => sizeof(double);

    protected override void InternalDeserialize(JsonReader reader)
    {
        if (reader.Read() == false)
        {
            throw new JsonSerializationException(
                "Unexpected end of JSON while reading a double value."
            );
        }

        if (reader.TokenType != JsonToken.Integer)
        {
            throw new JsonSerializationException(
                $"Expected a number token, but got {reader.TokenType}"
            );
        }

        if (reader.Value is long value)
        {
            _value = value;
        }
        else
        {
            throw new JsonSerializationException($"Unexpected value type: {reader.ValueType}");
        }
    }

    protected override void InternalSerialize(JsonWriter writer)
    {
        writer.WriteValue(_value);
    }

    public override string ToString() => $"{Value}";
}
