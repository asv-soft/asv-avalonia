using Asv.IO;
using Newtonsoft.Json;

namespace Asv.Avalonia;

public interface IJsonSerializable
{
    void Serialize(JsonWriter writer);
    void Deserialize(JsonReader reader);
}

public sealed class CommandSnapshot : ISizedSpanSerializable, IJsonSerializable
{
    private CommandArg _newValue;
    private CommandArg? _oldValue;

    public CommandSnapshot(
        string commandId,
        NavigationPath contextPath,
        CommandArg newValue,
        CommandArg? oldValue
    )
    {
        _newValue = newValue;
        _oldValue = oldValue;
        CommandId = commandId;
        ContextPath = contextPath;
    }

    public CommandSnapshot(JsonReader reader)
    {
        if (reader.Read() == false)
        {
            throw new JsonSerializationException("Unexpected end of JSON stream.");
        }

        if (reader.TokenType != JsonToken.StartArray)
        {
            throw new JsonSerializationException($"Expected {nameof(JsonToken.StartArray)} token.");
        }

        CommandId =
            reader.ReadAsString()
            ?? throw new JsonSerializationException($"{nameof(CommandId)} cannot be null.");
        ContextPath = new NavigationPath(reader);
        _newValue =
            CommandArg.Create(reader)
            ?? throw new JsonSerializationException($"{nameof(_newValue)} cannot be null.");
        _oldValue = CommandArg.Create(reader);

        if (reader.Read() == false || reader.TokenType != JsonToken.EndArray)
        {
            throw new JsonSerializationException($"Expected {nameof(JsonToken.EndArray)} token.");
        }
    }

    public string CommandId { get; private set; }
    public NavigationPath ContextPath { get; private set; }

    public CommandArg NewValue => _newValue;

    public CommandArg? OldValue => _oldValue;

    public override string ToString()
    {
        return $"{CommandId}[{ContextPath}]:({OldValue})=>({NewValue}))";
    }

    public void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        CommandId = BinSerialize.ReadString(ref buffer);
        ContextPath = new NavigationPath(ref buffer);
        _newValue = CommandArg.Create(ref buffer);
        if (BinSerialize.ReadBool(ref buffer))
        {
            _oldValue = CommandArg.Create(ref buffer);
        }
    }

    public void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteString(ref buffer, CommandId);
        ContextPath.Serialize(ref buffer);
        NewValue.Serialize(ref buffer);
        if (OldValue is null)
        {
            BinSerialize.WriteBool(ref buffer, false);
        }
        else
        {
            BinSerialize.WriteBool(ref buffer, true);
            OldValue.Serialize(ref buffer);
        }
    }

    public int GetByteSize()
    {
        return BinSerialize.GetSizeForString(CommandId)
            + ContextPath.GetByteSize()
            + NewValue.GetByteSize()
            + (OldValue is null ? sizeof(bool) : sizeof(bool) + OldValue.GetByteSize());
    }

    public void Serialize(JsonWriter writer)
    {
        writer.WriteStartArray();
        writer.WriteValue(CommandId);
        ContextPath.Serialize(writer);
        _newValue.Serialize(writer);
        if (_oldValue is not null)
        {
            _oldValue.Serialize(writer);
        }
        else
        {
            writer.WriteNull();
        }

        writer.WriteEndArray();
    }

    public void Deserialize(JsonReader reader)
    {
        throw new NotImplementedException(
            $"Not implemented for {nameof(CommandSnapshot)}. Use constructor with JsonReader parameter instead."
        );
    }
}
