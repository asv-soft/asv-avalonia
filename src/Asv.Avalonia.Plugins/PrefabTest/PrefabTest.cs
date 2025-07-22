using System.Composition;

namespace Asv.Avalonia.Plugins;

[ExportDialogPrefab]
[Shared]
[method: ImportingConstructor]
public sealed class PrefabTest() : IDialogPrefab<object, string?>
{
    public async Task<string?> ShowDialogAsync(object dialogPayload)
    {
        return await Task.FromResult<string?>(null);
    }
}
