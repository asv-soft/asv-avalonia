using Avalonia.Platform.Storage;

namespace Asv.Avalonia;

/// <summary>
/// Payload for SaveFileDialog prefab.
/// </summary>
public sealed class SaveFileDialogPayload
{
    /// <summary>
    /// Gets or inits the title of the dialog.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets or inits default extension of the file.
    /// </summary>
    public string? DefaultExt { get; init; }

    /// <summary>
    /// Gets or inits the extension filter, example: "txt, *, .nupkg".
    /// </summary>
    public string? TypeFilter { get; init; }

    /// <summary>
    /// Gets or inits directory where to start search.
    /// </summary>
    public string? InitialDirectory { get; init; }
}

/// <summary>
/// Dialog to save a file.
/// </summary>
public sealed class SaveFileDialogDesktopPrefab(IShellHost host)
    : IDialogPrefab<SaveFileDialogPayload, string?>
{
    public async Task<string?> ShowDialogAsync(SaveFileDialogPayload dialogPayload)
    {
        var options = new FilePickerSaveOptions { Title = dialogPayload.Title };

        if (!string.IsNullOrEmpty(dialogPayload.DefaultExt))
        {
            options.DefaultExtension = dialogPayload.DefaultExt;
        }

        if (!string.IsNullOrEmpty(dialogPayload.TypeFilter))
        {
            var fileTypes = dialogPayload
                .TypeFilter.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(ext => new FilePickerFileType(ext.Trim())
                {
                    Patterns = [$"*.{ext.Trim()}"], // linux, windows, web
                    AppleUniformTypeIdentifiers = null, // for apple
                    MimeTypes = null, // for web only
                })
                .ToList();

            options.FileTypeChoices = fileTypes;
        }

        if (!string.IsNullOrEmpty(dialogPayload.InitialDirectory))
        {
            options.SuggestedStartLocation =
                await host.TopLevel.StorageProvider.TryGetFolderFromPathAsync(
                    dialogPayload.InitialDirectory
                );
        }

        var result = await host.TopLevel.StorageProvider.SaveFilePickerAsync(options);
        return result?.Path.AbsolutePath;
    }
}
