using Newtonsoft.Json;

namespace Asv.Avalonia;

public partial class CommandArg
{
    public static CommandArg Empty => EmptyArg.Instance;

    public bool IsEmpty() => this is EmptyArg;
}

public class EmptyArg : CommandArg
{
    public static CommandArg Instance { get; } = new EmptyArg();

    protected internal static CommandArg CreateDefault() => Instance;

    public override Id TypeId => Id.Empty;

    protected override void InternalDeserialize(ref ReadOnlySpan<byte> buffer)
    {
        // No data to serialize
    }

    protected override void InternalSerialize(ref Span<byte> buffer)
    {
        // No data to serialize
    }

    protected override int InternalGetByteSize() => 0;

    protected override void InternalDeserialize(JsonReader reader)
    {
        if (reader.Read() == false || reader.TokenType != JsonToken.Null)
        {
            throw new JsonSerializationException("Expected null token for EmptyArg");
        }
    }

    protected override void InternalSerialize(JsonWriter writer)
    {
        writer.WriteNull();
    }

    public override string ToString() => "[EMPTY]";
}
