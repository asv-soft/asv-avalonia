using Asv.Common;
using Newtonsoft.Json;

namespace Asv.Avalonia;

public class JsonHistoryMetadata : IJsonSerializable
{
    public SemVersion Version { get; set; }
    public string Id { get; set; }
    public DateTimeOffset Updated { get; set; }

    public void Serialize(JsonWriter writer)
    {
        writer.WriteStartArray();
        writer.WriteValue(Version.ToString());
        writer.WriteValue(Id);
        writer.WriteValue(Updated);
        writer.WriteEndArray();
        writer.WriteRaw("\n");
    }

    public void Deserialize(JsonReader reader)
    {
        if (reader.Read() == false || reader.TokenType != JsonToken.StartArray)
        {
            throw new JsonSerializationException("Expected StartArray token.");
        }

        Version = SemVersion.Parse(
            reader.ReadAsString() ?? throw new JsonSerializationException("Version is null.")
        );
        Id = reader.ReadAsString() ?? throw new JsonSerializationException("StaticId is null.");
        Updated =
            reader.ReadAsDateTimeOffset()
            ?? throw new JsonSerializationException("Created is null.");

        if (reader.Read() == false || reader.TokenType != JsonToken.EndArray)
        {
            throw new JsonSerializationException("Expected EndArray token.");
        }
    }
}

public static class CommandHistoryFile
{
    private const string StaticHeader0 =
        "|============================================================================ |";
    private const string StaticHeader1 =
        "| This file contains command history in JSON format. Do not edit it manually. |";
    private const string StaticHeader2 =
        "| See more at https://github.com/asv-soft/asv-avalonia                        |";
    private static readonly SemVersion Version = new(1, 0, 0);

    public static void Save(string path, IEnumerable<CommandSnapshot> items, string id)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        using var sw = new JsonTextWriter(
            new StreamWriter(
                path,
                options: new FileStreamOptions
                {
                    Access = FileAccess.Write,
                    Mode = FileMode.CreateNew,
                }
            )
        );
        sw.Formatting = Formatting.None;
        sw.CloseOutput = true;
        var index = 0;
        var metadata = new JsonHistoryMetadata
        {
            Version = Version.ToString(),
            Id = id,
            Updated = DateTimeOffset.UtcNow,
        };
        sw.WriteComment(StaticHeader0);
        sw.WriteRaw("\n");
        sw.WriteComment(StaticHeader1);
        sw.WriteRaw("\n");
        sw.WriteComment(StaticHeader2);
        sw.WriteRaw("\n");
        sw.WriteComment(StaticHeader0);
        sw.WriteRaw("\n");
        metadata.Serialize(sw);
        foreach (var cmd in items)
        {
            sw.WriteComment($"{index:000}");
            cmd.Serialize(sw);
            sw.WriteComment($"{index:000}");
            sw.WriteRaw("\n");
            ++index;
        }

        sw.Flush();
    }

    public static IEnumerable<CommandSnapshot> Load(
        string path,
        Action<JsonHistoryMetadata>? checkMetadata
    )
    {
        if (File.Exists(path))
        {
            using var rdr = new JsonTextReader(new StreamReader(path))
            {
                SupportMultipleContent = true,
            };
            for (var i = 0; i < 4; i++)
            {
                if (rdr.Read() == false || rdr.TokenType != JsonToken.Comment)
                {
                    throw new JsonSerializationException(
                        "Expected a comment at the start of the file."
                    );
                }
            }

            var metadata = new JsonHistoryMetadata();
            metadata.Deserialize(rdr);
            checkMetadata?.Invoke(metadata);

            while (rdr.Read())
            {
                if (rdr.TokenType != JsonToken.Comment)
                {
                    throw new JsonSerializationException(
                        "Expected a comment before the command snapshot."
                    );
                }

                yield return new CommandSnapshot(rdr);

                if (rdr.Read() == false || rdr.TokenType != JsonToken.Comment)
                {
                    throw new JsonSerializationException(
                        "Expected a comment at the end of the command snapshot."
                    );
                }
            }
        }
    }
}
