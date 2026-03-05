using Asv.Common;

namespace Asv.Avalonia.Plugins;

public class NullPluginBootloader : IPluginBootloader
{
    public static IPluginBootloader Instance { get; } = new NullPluginBootloader(); 
    private NullPluginBootloader()
    {
    }
    public SemVersion ApiVersion { get; } = new(0, 0, 0);
    public IEnumerable<ILocalPluginInfo> Installed { get; } = [];
}