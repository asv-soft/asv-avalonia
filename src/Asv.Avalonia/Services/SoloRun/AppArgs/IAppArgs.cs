using System.Collections.Immutable;

namespace Asv.Avalonia;

public interface IAppArgs
{
    IReadOnlyDictionary<string, string> Args { get; }
    IReadOnlyList<string> Tags { get; }
    string this[string key, string defaultValue] { get; }
}
