using System.Composition.Hosting;
using System.Composition.Hosting.Core;

namespace Asv.Avalonia;

public class ProxyExportDescriptorProvider : ExportDescriptorProvider
{
    private readonly CompositionHost _sourceContainer;

    public ProxyExportDescriptorProvider(CompositionHost sourceContainer)
    {
        _sourceContainer = sourceContainer;
    }

    public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors(
        CompositionContract contract,
        DependencyAccessor descriptorAccessor
    )
    {
        if (_sourceContainer.TryGetExport(contract.ContractType, out var export))
        {
            yield return new ExportDescriptorPromise(
                contract,
                "ProxyExportDescriptorProvider",
                true,
                Array.Empty<CompositionDependency>,
                deps => ExportDescriptor.Create((c, o) => export, new Dictionary<string, object>())
            );
        }
    }
}
