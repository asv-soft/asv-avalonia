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
public sealed class SelectFolderDialogDesktopPrefab
    : IDialogPrefab<SelectFolderDialogPayload, string?>
{
    public async Task<string?> ShowDialogAsync(SelectFolderDialogPayload dialogPayload)
    {
        var topLevel = TopLevelHelper.GetTopLevel();
        if (topLevel is null)
        {
            return null;
        }

        var options = new FolderPickerOpenOptions
        {
            Title = dialogPayload.Title,
            AllowMultiple = false,
        };

        if (!string.IsNullOrEmpty(dialogPayload.OldPath))
        {
            options.SuggestedStartLocation =
                await topLevel.StorageProvider.TryGetFolderFromPathAsync(dialogPayload.OldPath);
        }

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(options);

        var folder = folders.FirstOrDefault()?.Path.AbsolutePath;

        return folder;
    }
}
