using NuGet.Protocol.Core.Types;

namespace Asv.Avalonia;

public class SourceInfo(SourceRepository sourceRepository) : IPluginServerInfo
{
    public string SourceUri => sourceRepository.PackageSource.Source;
    public string Name => sourceRepository.PackageSource.Name;
    public string? Username => sourceRepository.PackageSource.Credentials?.Username;
}
