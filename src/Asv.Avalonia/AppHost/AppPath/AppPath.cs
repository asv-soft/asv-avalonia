using Microsoft.Extensions.Options;

namespace Asv.Avalonia;

public class AppPathConfig
{
    public const string Section = "Path";
    public string UserDataFolder { get; set; } = "data";
}

public class AppPath : IAppPath
{
    public AppPath(IOptions<AppPathConfig> config)
    {
        UserDataFolder = config.Value.UserDataFolder;
        if (Directory.Exists(UserDataFolder) == false)
        {
            Directory.CreateDirectory(UserDataFolder);
        }
    }

    public string UserDataFolder { get; }
}
