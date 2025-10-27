using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Asv.Avalonia;

public partial class AppArgs : IAppArgs
{
    private readonly ImmutableDictionary<string, string> _args;
    private readonly ImmutableArray<string> _tags;

    [GeneratedRegex(@"^--([^=]+)=(.*)$", RegexOptions.Compiled)]
    private static partial Regex ArgsParserRegex();

    private AppArgs(ImmutableDictionary<string, string> keys, ImmutableArray<string> values)
    {
        _args = keys;
        _tags = values;
    }

    public AppArgs(string[] args)
    {
        var keyValuePattern = ArgsParserRegex();
        var builder = ImmutableDictionary.CreateBuilder<string, string>();
        var tagBuilder = ImmutableArray.CreateBuilder<string>();

        foreach (var arg in args)
        {
            var match = keyValuePattern.Match(arg);
            if (match.Success)
            {
                var key = match.Groups[1].Value;
                var value = match.Groups[2].Value;
                builder.Add(key, value);
            }
            else
            {
                tagBuilder.Add(arg);
            }
        }

        _args = builder.ToImmutable();
        _tags = tagBuilder.ToImmutable();
    }

    public IReadOnlyDictionary<string, string> Args => _args;

    public IReadOnlyList<string> Tags => _tags;

    public string this[string key, string defaultValue] =>
        _args.GetValueOrDefault(key, defaultValue);

    #region Serialization

    private class SerializationModel
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        public Dictionary<string, string> Keys { get; set; } = new();

        // ReSharper disable once CollectionNeverUpdated.Local
        public List<string> Tags { get; set; } = [];
    }

    #endregion
    public static AppArgs DeserializeFromString(string sourceString)
    {
        var model = JsonConvert.DeserializeObject<SerializationModel>(sourceString);
        var builder = ImmutableDictionary.CreateBuilder<string, string>();
        var tagBuilder = ImmutableArray.CreateBuilder<string>();

        if (model != null)
        {
            foreach (var (key, value) in model.Keys)
            {
                builder.Add(key, value);
            }

            foreach (var tag in model.Tags)
            {
                tagBuilder.Add(tag);
            }
        }

        return new AppArgs(builder.ToImmutable(), tagBuilder.ToImmutable());
    }

    public string SerializeToString()
    {
        var result = JsonConvert.SerializeObject(
            new SerializationModel
            {
                Keys = _args.ToDictionary(x => x.Key, x => x.Value),
                Tags = _tags.ToList(),
            },
            Formatting.None
        );
        return result;
    }
}
