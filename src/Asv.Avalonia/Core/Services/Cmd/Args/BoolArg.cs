using System.Runtime.CompilerServices;
using Asv.IO;
using Newtonsoft.Json;

namespace Asv.Avalonia;

public partial class CommandArg
{
    public static CommandArg True => new BoolArg(true);
    public static CommandArg False => new BoolArg(false);

    public static explicit operator bool(CommandArg value)
    {
        if (value is BoolArg boolArg)
        {
            return boolArg.Value;
        }

        throw new InvalidCastException($"Cannot cast {value.GetType().Name} to {nameof(Boolean)}");
    }

    public static implicit operator CommandArg(bool value)
    {
        return new BoolArg(value);
    }

    public static BoolArg CreateBool(bool value)
    {
        return new BoolArg(value);
    }

    public bool AsBool()
    {
        if (this is BoolArg arg)
        {
            return arg.Value;
        }

        throw new InvalidCastException($"Cannot cast {GetType().Name} to {nameof(Boolean)}");
    }
}

public class BoolArg(bool value) : CommandArg
{
    #region Static

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal static CommandArg CreateDefault() => new BoolArg(false);

    #endregion

    private bool _value = value;

    public override Id TypeId => CommandArg.Id.Bool;
    public bool Value => _value;

    public override string ToString() => $"{Value}";

    protected override void InternalDeserialize(ref ReadOnlySpan<byte> buffer) =>
        BinSerialize.ReadBool(ref buffer, ref _value);

    protected override void InternalSerialize(ref Span<byte> buffer) =>
        BinSerialize.WriteBool(ref buffer, _value);

    protected override int InternalGetByteSize() => sizeof(bool);

    protected override void InternalDeserialize(JsonReader reader)
    {
        _value =
            reader.ReadAsBoolean()
            ?? throw new JsonSerializationException("Expected a boolean value.");
    }

    protected override void InternalSerialize(JsonWriter writer)
    {
        writer.WriteValue(_value);
    }
}
