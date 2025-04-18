using System.Composition;
using Avalonia.Platform.Storage;

namespace Asv.Avalonia;

/// <summary>
/// Payload for SelectFolderDialog prefab.
/// </summary>
public sealed class SelectFolderDialogPayload
{
    /// <summary>
    /// Gets or inits the title of the dialog.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets or inits the default path.
    /// </summary>
    public string? OldPath { get; init; }
}

/// <summary>
/// Dialog to select a folder.
/// </summary>
[ExportDialogPrefab]
[Shared]
[method: ImportingConstructor]
public sealed class SelectFolderDialogDesktopPrefab(IShellHost host)
    : IDialogPrefab<SelectFolderDialogPayload, string?>
{
    public async Task<string?> ShowDialogAsync(SelectFolderDialogPayload dialogPayload)
    {
        var options = new FolderPickerOpenOptions
        {
            Title = dialogPayload.Title,
            AllowMultiple = false,
        };

        if (!string.IsNullOrEmpty(dialogPayload.OldPath))
        {
            options.SuggestedStartLocation =
                await host.TopLevel.StorageProvider.TryGetFolderFromPathAsync(
                    dialogPayload.OldPath
                );
        }

        var folders = await host.TopLevel.StorageProvider.OpenFolderPickerAsync(options);

        var folder = folders.FirstOrDefault()?.Path.AbsolutePath;

        return folder;
    }
}
