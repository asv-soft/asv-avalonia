using Microsoft.Extensions.Options;

namespace Asv.Avalonia;

public class UserJsonConfigurationBuilder
{
    private string _fileName;
    private TimeSpan _autoSaveInterval;

    internal UserJsonConfigurationBuilder()
    {
        _fileName = "user_settings.json";
    }

    internal void Build(OptionsBuilder<UserConfigurationOptions> options)
    {
        options.Configure<IAppPath>(
            (cfg, path) =>
            {
                cfg.AutoSaveMs = (int)_autoSaveInterval.TotalMilliseconds;
                cfg.FilePath = Path.Combine(path.UserDataFolder, _fileName);
            }
        );
    }

    public UserJsonConfigurationBuilder WithFileName(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        _fileName = fileName;
        return this;
    }

    public UserJsonConfigurationBuilder WithAutoSave(TimeSpan interval)
    {
        _autoSaveInterval = interval;
        return this;
    }
}
