using Asv.Cfg;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asv.Avalonia;

public class UserConfigurationOptions
{
    public const string SectionName = "UserConfiguration";
    public int AutoSaveMs { get; set; } = 500;
}

public class UserJsonConfiguration : JsonOneFileConfiguration
{
    private const string UserSettingsFileName = "settings.json";
    public UserJsonConfiguration(
        IOptions<UserConfigurationOptions> config,
        IAppPath path,
        ILoggerFactory loggerFactory
    )
        : base(
            path.GetAppPathFile(UserSettingsFileName),
            true,
            config.Value.AutoSaveMs <= 0
                ? null
                : TimeSpan.FromMilliseconds(config.Value.AutoSaveMs),
            true,
            loggerFactory.CreateLogger<UserJsonConfiguration>()
        ) { }
}
