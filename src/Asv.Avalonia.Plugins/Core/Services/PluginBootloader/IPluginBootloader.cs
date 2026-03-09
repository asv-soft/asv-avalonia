using System.Runtime.Loader;
using Asv.Common;
using Microsoft.Extensions.FileProviders;
using R3;

namespace Asv.Avalonia.Plugins;

public interface IPluginBootloader
{
    SemVersion ApiVersion { get; }
    IEnumerable<ILocalPluginInfo> Installed { get; }
}
