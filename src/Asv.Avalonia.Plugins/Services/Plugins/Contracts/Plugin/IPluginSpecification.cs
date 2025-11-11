using Asv.Common;

namespace Asv.Avalonia.Plugins;

public interface IPluginSpecification
{
    SemVersion ApiVersion { get; }
    string PackageId { get; }
    string? Title { get; }
    public string? Description { get; }
    string? Authors { get; }
    string? Tags { get; }
    bool IsVerified { get; }
}
