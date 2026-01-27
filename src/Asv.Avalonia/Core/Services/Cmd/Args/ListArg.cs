using System.Collections;
using Asv.IO;
using Newtonsoft.Json;

namespace Asv.Avalonia;

public partial class CommandArg
{
    public static ListArg CreateList(params IEnumerable<CommandArg> value)
    {
        return new ListArg(value);
    }

    public ListArg AsList()
    {
        if (this is ListArg list)
        {
            return list;
        }

        throw new ArgumentException(
            $"Cannot convert {this.GetType().Name} to ListArg. Expected a ListArg."
        );
    }
}

public class ListArg : CommandArg, IList<CommandArg>
{
    protected internal static CommandArg CreateDefault() => new ListArg();

    private readonly List<CommandArg> _items;

    public ListArg() => _items = [];

    public ListArg(int capacity) => _items = new List<CommandArg>(capacity);

    public ListArg(params IEnumerable<CommandArg> items)
    {
        if (items is null)
        {
            throw new ArgumentNullException(nameof(items), "Items cannot be null.");
        }

        _items = new List<CommandArg>(items);
    }

    public override Id TypeId => Id.List;

    protected override void InternalDeserialize(ref ReadOnlySpan<byte> buffer)
    {
        var size = BinSerialize.ReadPackedUnsignedInteger(ref buffer);
        Clear();
        for (var i = 0; i < size; i++)
        {
            var element = Create(ref buffer);
            element.Deserialize(ref buffer);
            Add(element);
        }
    }

    protected override void InternalSerialize(ref Span<byte> buffer)
    {
        var size = Count; // Assuming Count is the number of elements in the list
        BinSerialize.WritePackedUnsignedInteger(ref buffer, (uint)size);
        foreach (var item in this)
        {
            item.Serialize(ref buffer);
        }
    }

    protected override int InternalGetByteSize() =>
        BinSerialize.GetSizeForPackedUnsignedInteger((uint)Count)
        + this.Sum(item => item.GetByteSize());

    protected override void InternalDeserialize(JsonReader reader)
    {
        if (reader.Read() == false || reader.TokenType != JsonToken.StartArray)
        {
            throw new JsonSerializationException("Expected start of array.");
        }

        Clear();
        while (reader.Read() && reader.TokenType != JsonToken.EndArray)
        {
            var item =
                InternalCreateWithReadedToken(reader)
                ?? throw new JsonSerializationException("Failed to create CommandArg from JSON.");
            Add(item);
        }

        if (reader.TokenType != JsonToken.EndArray)
        {
            throw new JsonSerializationException("Expected end of array.");
        }
    }

    protected override void InternalSerialize(JsonWriter writer)
    {
        writer.WriteStartArray();
        foreach (var item in this)
        {
            item.Serialize(writer);
        }

        writer.WriteEndArray();
    }

    public IEnumerator<CommandArg> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(CommandArg item) => _items.Add(item);

    public void Clear() => _items.Clear();

    public bool Contains(CommandArg item) => _items.Contains(item);

    public void CopyTo(CommandArg[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    public bool Remove(CommandArg item) => _items.Remove(item);

    public int Count => _items.Count;
    public bool IsReadOnly => false;

    public int IndexOf(CommandArg item) => _items.IndexOf(item);

    public void Insert(int index, CommandArg item) => _items.Insert(index, item);

    public void RemoveAt(int index) => _items.RemoveAt(index);

    public CommandArg this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    public override string ToString() => $"[{string.Join(", ", _items.Select(x => x.ToString()))}]";
}
