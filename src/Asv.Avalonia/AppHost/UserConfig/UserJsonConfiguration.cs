using Asv.Cfg;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asv.Avalonia;

public class UserConfigurationConfig
{
    public string FilePath { get; set; } = "user_settings.json";
    public int AutoSaveMs { get; set; } = 0;
}

public class UserJsonConfiguration : JsonOneFileConfiguration
{
    public UserJsonConfiguration(
        IOptions<UserConfigurationConfig> config,
        ILoggerFactory loggerFactory
    )
        : base(
            config.Value.FilePath,
            true,
            config.Value.AutoSaveMs <= 0
                ? null
                : TimeSpan.FromMilliseconds(config.Value.AutoSaveMs),
            true,
            loggerFactory.CreateLogger<UserJsonConfiguration>()
        ) { }
}
