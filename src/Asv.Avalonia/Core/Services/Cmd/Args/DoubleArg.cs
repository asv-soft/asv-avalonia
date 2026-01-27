using Asv.IO;
using Newtonsoft.Json;

namespace Asv.Avalonia;

public partial class CommandArg
{
    public static CommandArg Double(double value)
    {
        return new DoubleArg(value);
    }

    public static explicit operator double(CommandArg value)
    {
        if (value is DoubleArg arg)
        {
            return arg.Value;
        }

        throw new InvalidCastException($"Cannot cast {value.GetType().Name} to {nameof(Double)}");
    }

    public static implicit operator CommandArg(double value)
    {
        return new DoubleArg(value);
    }

    public static DoubleArg CreateDouble(double value)
    {
        return new DoubleArg(value);
    }

    public double AsDouble()
    {
        if (this is DoubleArg arg)
        {
            return arg.Value;
        }

        throw new InvalidCastException($"Cannot cast {GetType().Name} to {nameof(Double)}");
    }
}

public class DoubleArg(double value) : CommandArg
{
    protected internal static CommandArg CreateDefault() => new DoubleArg(double.NaN);

    private double _value = value;
    public double Value => _value;

    public override Id TypeId => Id.Double;

    protected override void InternalDeserialize(ref ReadOnlySpan<byte> buffer) =>
        BinSerialize.ReadDouble(ref buffer, ref _value);

    protected override void InternalSerialize(ref Span<byte> buffer) =>
        BinSerialize.WriteDouble(ref buffer, in _value);

    protected override int InternalGetByteSize() => sizeof(double);

    protected override void InternalDeserialize(JsonReader reader)
    {
        if (reader.Read() == false)
        {
            throw new JsonSerializationException(
                "Unexpected end of JSON while reading a double value."
            );
        }

        if (reader.TokenType != JsonToken.Float && reader.TokenType != JsonToken.Integer)
        {
            throw new JsonSerializationException(
                $"Expected a number token, but got {reader.TokenType}"
            );
        }

        if (reader.Value is double value)
        {
            _value = value;
        }
        else if (reader.Value is long longValue)
        {
            _value = longValue;
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
