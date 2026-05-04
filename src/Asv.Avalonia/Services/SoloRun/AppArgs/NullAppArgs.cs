namespace Asv.Avalonia;

public class NullAppArgs : IAppArgs
{
    public static IAppArgs Instance { get; } = new NullAppArgs();

    public IReadOnlyDictionary<string, string> Args { get; } = new Dictionary<string, string>();
    public IReadOnlyList<string> Tags { get; } = [];

    public string this[string key, string defaultValue] => string.Empty;
}
