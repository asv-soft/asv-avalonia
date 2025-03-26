using System.Buffers;
using Newtonsoft.Json;

namespace Asv.Avalonia;

public class EmptyCommandArg : ICommandArg
{
    public ValueTask Save(in IBufferWriter<byte> buffer)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask Restore(in ReadOnlySequence<byte> buffer)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask Save(JsonWriter wrt)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask Restore(JsonReader rdr)
    {
        return ValueTask.CompletedTask;
    }

    public override string ToString() => "[EMPTY]";
}
