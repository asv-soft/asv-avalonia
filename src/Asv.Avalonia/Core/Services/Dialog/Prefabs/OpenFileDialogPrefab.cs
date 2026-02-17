using Avalonia.Platform.Storage;

namespace Asv.Avalonia;

/// <summary>
/// Payload for OpenFileDialog prefab.
/// </summary>
public sealed class OpenFileDialogPayload
{
    /// <summary>
    /// Gets or inits the title of the dialog.
    /// </summary>
    public required string Title { get; init; }

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
/// Dialog to select a file.
/// </summary>
public sealed class OpenFileDialogDesktopPrefab(IShellHost host)
    : IDialogPrefab<OpenFileDialogPayload, string?>
{
    public async Task<string?> ShowDialogAsync(OpenFileDialogPayload dialogPayload)
    {
        var options = new FilePickerOpenOptions
        {
            Title = dialogPayload.Title,
            AllowMultiple = false,
        };
        if (!string.IsNullOrEmpty(dialogPayload.TypeFilter))
        {
            var fileTypes = dialogPayload
                .TypeFilter.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(ext =>
                {
                    if (ext == "*")
                    {
                        return new FilePickerFileType(ext.Trim())
                        {
                            Patterns = ["*"], // linux, windows, web
                            AppleUniformTypeIdentifiers = null, // for apple
                            MimeTypes = null, // for web only
                        };
                    }

                    return new FilePickerFileType(ext.Trim())
                    {
                        Patterns = [$"*.{ext.Trim()}"], // linux, windows, web
                        AppleUniformTypeIdentifiers = null, // for apple
                        MimeTypes = null, // for web only
                    };
                })
                .ToList();

            options.FileTypeFilter = fileTypes;
        }

        if (!string.IsNullOrEmpty(dialogPayload.InitialDirectory))
        {
            options.SuggestedStartLocation =
                await host.TopLevel.StorageProvider.TryGetFolderFromPathAsync(
                    dialogPayload.InitialDirectory
                );
        }

        var files = await host.TopLevel.StorageProvider.OpenFilePickerAsync(options);

        return files.Count == 1 ? files[0].Path.AbsolutePath : null;
    }
}
