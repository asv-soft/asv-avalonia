namespace Asv.Avalonia;

public interface IAppArgs
{
    IReadOnlyDictionary<string, string> Args { get; }
    IReadOnlySet<string> Tags { get; }
    string this[string key, string defaultValue] { get; }
}
