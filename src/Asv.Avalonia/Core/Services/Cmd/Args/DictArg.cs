using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Asv.IO;
using Newtonsoft.Json;

namespace Asv.Avalonia;

public partial class CommandArg
{
    public static DictArg CreateDictionary(int capacity)
    {
        return new DictArg(capacity);
    }

    public static DictArg CreateDictionary()
    {
        return new DictArg();
    }

    public static DictArg CreateDictionary(IDictionary<string, CommandArg> dictionary)
    {
        return new DictArg(dictionary);
    }

    public static DictArg CreateDictionary(
        params IEnumerable<KeyValuePair<string, CommandArg>> collection
    )
    {
        return new DictArg(collection);
    }

    public DictArg AsDictionary()
    {
        if (this is DictArg list)
        {
            return list;
        }

        throw new ArgumentException(
            $"Cannot convert {GetType().Name} to {nameof(DictArg)}. Expected a dictionary type."
        );
    }
}

public class DictArg : CommandArg, IDictionary<string, CommandArg>
{
    public static IEqualityComparer<string> Comparer => StringComparer.InvariantCultureIgnoreCase;

    protected internal static CommandArg CreateDefault() => new DictArg();

    private readonly Dictionary<string, CommandArg> _dictionary;

    public DictArg()
    {
        _dictionary = new(Comparer);
    }

    public DictArg(int capacity)
    {
        _dictionary = new(capacity, Comparer);
    }

    public DictArg(IDictionary<string, CommandArg> dictionary)
    {
        _dictionary = new(dictionary, Comparer);
    }

    public DictArg(IEnumerable<KeyValuePair<string, CommandArg>> collection)
    {
        _dictionary = new(collection, Comparer);
    }

    public override Id TypeId => Id.Dict;

    protected override void InternalDeserialize(ref ReadOnlySpan<byte> buffer)
    {
        var size = BinSerialize.ReadPackedUnsignedInteger(ref buffer);
        Clear();
        for (var i = 0; i < size; i++)
        {
            var key = BinSerialize.ReadString(ref buffer);
            var element = Create(ref buffer);
            element.Deserialize(ref buffer);
            Add(key, element);
        }
    }

    protected override void InternalSerialize(ref Span<byte> buffer)
    {
        var size = _dictionary.Count;
        BinSerialize.WritePackedUnsignedInteger(ref buffer, (uint)size);
        foreach (var kvp in _dictionary)
        {
            BinSerialize.WriteString(ref buffer, kvp.Key);
            kvp.Value.Serialize(ref buffer);
        }
    }

    protected override int InternalGetByteSize() =>
        BinSerialize.GetSizeForPackedUnsignedInteger((uint)_dictionary.Count)
        + _dictionary.Sum(kvp => BinSerialize.GetSizeForString(kvp.Key) + kvp.Value.GetByteSize());

    protected override void InternalDeserialize(JsonReader reader)
    {
        if (reader.Read() == false || reader.TokenType != JsonToken.StartObject)
        {
            throw new JsonSerializationException("Expected start of object.");
        }

        Clear();
        while (reader.Read() && reader.TokenType != JsonToken.EndObject)
        {
            if (reader.TokenType != JsonToken.PropertyName)
            {
                throw new JsonSerializationException("Expected property name.");
            }

            var key =
                reader.Value?.ToString()
                ?? throw new JsonSerializationException("Key cannot be null.");

            var element = Create(reader);
            if (element == null)
            {
                throw new JsonSerializationException("Element cannot be null.");
            }

            Add(key, element);
        }

        if (reader.TokenType != JsonToken.EndObject)
        {
            throw new JsonSerializationException("Expected end of object.");
        }
    }

    protected override void InternalSerialize(JsonWriter writer)
    {
        writer.WriteStartObject();
        foreach (var kvp in _dictionary)
        {
            writer.WritePropertyName(kvp.Key);
            kvp.Value.Serialize(writer);
        }

        writer.WriteEndObject();
    }

    public IEnumerator<KeyValuePair<string, CommandArg>> GetEnumerator() =>
        _dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(KeyValuePair<string, CommandArg> item) => _dictionary.Add(item.Key, item.Value);

    public void Clear() => _dictionary.Clear();

    public bool Contains(KeyValuePair<string, CommandArg> item) => _dictionary.Contains(item);

    public void CopyTo(KeyValuePair<string, CommandArg>[] array, int arrayIndex) =>
        ((ICollection<KeyValuePair<string, CommandArg>>)_dictionary).CopyTo(array, arrayIndex);

    public bool Remove(KeyValuePair<string, CommandArg> item) => _dictionary.Remove(item.Key);

    public int Count => _dictionary.Count;
    public bool IsReadOnly => false;

    public void Add(string key, CommandArg value) => _dictionary.Add(key, value);

    public bool ContainsKey(string key) => _dictionary.ContainsKey(key);

    public bool Remove(string key) => _dictionary.Remove(key);

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out CommandArg value) =>
        _dictionary.TryGetValue(key, out value);

    public CommandArg this[string key]
    {
        get => _dictionary[key];
        set => _dictionary[key] = value;
    }

    public ICollection<string> Keys => _dictionary.Keys;
    public ICollection<CommandArg> Values => _dictionary.Values;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("{ ");
        foreach (var kvp in _dictionary)
        {
            sb.AppendFormat("{0}: {1}, ", kvp.Key, kvp.Value);
        }

        if (sb.Length > 2)
        {
            sb.Length -= 2; // Remove the last comma and space
        }

        sb.Append(" }");
        return sb.ToString();
    }
}
