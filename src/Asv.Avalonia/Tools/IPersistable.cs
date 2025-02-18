using System.Buffers;
using MemoryPack;
using Newtonsoft.Json;

namespace Asv.Avalonia;

public interface IPersistable
{
    ValueTask Save(in IBufferWriter<byte> buffer);
    ValueTask Restore(in ReadOnlySequence<byte> buffer);
    ValueTask Save(JsonWriter wrt);
    ValueTask Restore(JsonReader rdr);
}

[MemoryPackable]
[method: MemoryPackConstructor]
public partial class Persistable<T>(T value) : IPersistable
{
    public T Value { get; set; } = value;

    public ValueTask Save(in IBufferWriter<byte> buffer)
    {
        var those = this;
        MemoryPackSerializer.Serialize(in buffer, in those);
        return ValueTask.CompletedTask;
    }

    public ValueTask Restore(in ReadOnlySequence<byte> buffer)
    {
        var those = this;
        MemoryPackSerializer.Deserialize(in buffer, ref those);
        return ValueTask.CompletedTask;
    }

    public ValueTask Save(JsonWriter wrt)
    {
        wrt.WriteValue(Value);
        return ValueTask.CompletedTask;
    }

    public ValueTask Restore(JsonReader rdr)
    {
        if (rdr.Read() == false || rdr.Value == null)
        {
            return ValueTask.FromException(new Exception("Error to deserialize"));
        }

        Value = (T)rdr.Value;
        return ValueTask.CompletedTask;
    }
}

[MemoryPackable]
[method: MemoryPackConstructor]
public partial class PersistableChange<T>(T oldValue, T newValue) : IPersistable
{
    public T OldValue { get; set; } = oldValue;
    public T NewValue { get; set; } = newValue;

    public ValueTask Save(in IBufferWriter<byte> buffer)
    {
        var those = this;
        MemoryPackSerializer.Serialize(in buffer, in those);
        return ValueTask.CompletedTask;
    }

    public ValueTask Restore(in ReadOnlySequence<byte> buffer)
    {
        var those = this;
        MemoryPackSerializer.Deserialize(in buffer, ref those);
        return ValueTask.CompletedTask;
    }

    public ValueTask Save(JsonWriter wrt)
    {
        wrt.WriteValue(NewValue);
        wrt.WriteValue(OldValue);
        return ValueTask.CompletedTask;
    }

    public ValueTask Restore(JsonReader rdr)
    {
        if (rdr.Read() == false || rdr.Value == null)
        {
            return ValueTask.FromException(new Exception("Error to deserialize"));
        }

        NewValue = (T)rdr.Value;

        if (rdr.Read() == false || rdr.Value == null)
        {
            return ValueTask.FromException(new Exception("Error to deserialize"));
        }

        OldValue = (T)rdr.Value;
        return ValueTask.CompletedTask;
    }
}
