namespace Asv.Avalonia;

public class NullAppPath : IAppPath
{
    public static IAppPath Instance { get; } = new NullAppPath();

    private NullAppPath() { }

    public string UserDataFolder => "data";
}
