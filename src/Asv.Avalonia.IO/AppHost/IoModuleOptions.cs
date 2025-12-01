namespace Asv.Avalonia.IO;

public class IoModuleOptions
{
    public const string ConfigurationSection = "IoModule";

    public bool IsEnabled { get; set; } = false;

    public bool IsDeviceFeatureEnabled { get; set; } = false;
}
