using Microsoft.Extensions.Options;

namespace Asv.Avalonia;

public class AppPathBuilder
{
    private string _folderName;

    internal AppPathBuilder()
    {
        _folderName = string.Empty;
    }

    public AppPathBuilder WithSpecialFolder(Environment.SpecialFolder folder, string folderName)
    {
        ArgumentNullException.ThrowIfNull(folderName);
        _folderName = Path.Combine(Environment.GetFolderPath(folder), folderName);
        return this;
    }

    public AppPathBuilder WithRelativeFolder(string folderName)
    {
        ArgumentNullException.ThrowIfNull(folderName);
        _folderName = Path.GetFullPath(folderName);
        return this;
    }

    internal void Build(OptionsBuilder<AppPathConfig> options)
    {
        options.Configure(cfg =>
        {
            cfg.UserDataFolder = _folderName;
        });
    }
}
