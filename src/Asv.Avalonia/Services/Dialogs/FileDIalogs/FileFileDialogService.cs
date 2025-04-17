using System.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Platform.Storage;

namespace Asv.Avalonia;

[Export(typeof(IFileDialogService))]
[Shared]
public sealed class FileFileDialogService : IFileDialogService
{
    private readonly IShellHost _host;
    private readonly INavigationService _navigation;

    [ImportingConstructor]
    public FileFileDialogService(IShellHost host, INavigationService navigationService)
    {
        _host = host;
        _navigation = navigationService;
    }

    public IShellHost ShellHost { get; set; }
    public bool IsImplementedShowOpenFileDialog { get; } = true;
    public bool IsImplementedShowSaveFileDialog { get; } = true;
    public bool IsImplementedShowSelectFolderDialog { get; } = true;
    public bool IsImplementedShowObserveFolderDialog { get; } = true;

    /// <summary>
    /// Desktop only.
    /// </summary>
    /// <param name="title">.</param>
    /// <param name="typeFilter">.</param>
    /// <param name="initialDirectory">.</param>
    /// <returns>.</returns>
    public async Task<string?> ShowOpenFileDialog(
        string title,
        string? typeFilter = null,
        string? initialDirectory = null
    )
    {
        var options = new FilePickerOpenOptions { Title = title, AllowMultiple = false };
        if (!string.IsNullOrEmpty(typeFilter))
        {
            var fileTypes = typeFilter
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
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

        if (!string.IsNullOrEmpty(initialDirectory))
        {
            options.SuggestedStartLocation =
                await _host.TopLevel.StorageProvider.TryGetFolderFromPathAsync(initialDirectory);
        }

        var files = await _host.TopLevel.StorageProvider.OpenFilePickerAsync(options);

        return files.Count == 1 ? files[0].Path.AbsolutePath : null;
    }

    /// <summary>
    /// Desktop only.
    /// </summary>
    /// <param name="title">.</param>
    /// <param name="defaultExt">.</param>
    /// <param name="typeFilter">.</param>
    /// <param name="initialDirectory">.</param>
    /// <returns>.</returns>
    public async Task<string?> ShowSaveFileDialog(
        string title,
        string? defaultExt = null,
        string? typeFilter = null,
        string? initialDirectory = null
    )
    {
        var options = new FilePickerSaveOptions { Title = title };

        if (!string.IsNullOrEmpty(defaultExt))
        {
            options.DefaultExtension = defaultExt;
        }

        if (!string.IsNullOrEmpty(typeFilter))
        {
            var fileTypes = typeFilter
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(ext => new FilePickerFileType(ext.Trim())
                {
                    Patterns = [$"*.{ext.Trim()}"], // linux, windows, web
                    AppleUniformTypeIdentifiers = null, // for apple
                    MimeTypes = null, // for web only
                })
                .ToList();

            options.FileTypeChoices = fileTypes;
        }

        if (!string.IsNullOrEmpty(initialDirectory))
        {
            options.SuggestedStartLocation =
                await _host.TopLevel.StorageProvider.TryGetFolderFromPathAsync(initialDirectory);
        }

        var result = await _host.TopLevel.StorageProvider.SaveFilePickerAsync(options);
        return result?.Path.AbsolutePath;
    }

    /// <summary>
    /// Desktop only.
    /// </summary>
    /// <param name="title">.</param>
    /// <param name="oldPath">.</param>
    /// <returns>.</returns>
    public async Task<string?> ShowSelectFolderDialog(string title, string? oldPath = null)
    {
        var options = new FolderPickerOpenOptions { Title = title, AllowMultiple = false };

        if (!string.IsNullOrEmpty(oldPath))
        {
            options.SuggestedStartLocation =
                await _host.TopLevel.StorageProvider.TryGetFolderFromPathAsync(oldPath);
        }

        var folders = await _host.TopLevel.StorageProvider.OpenFolderPickerAsync(options);

        var folder = folders.FirstOrDefault()?.Path.AbsolutePath;

        return folder;
    }

    /// <summary>
    /// Desktop only.
    /// </summary>
    /// <param name="title">.</param>
    /// <param name="defaultPath">.</param>
    /// <returns>.</returns>
    public Task ShowObserveFolderDialog(string title, string? defaultPath = null)
    {
        if (defaultPath is null)
        {
            return Task.CompletedTask;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            OpenFolderInWindowsExplorer(defaultPath);
            return Task.CompletedTask;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            OpenFolderInMacFinder(defaultPath);
            return Task.CompletedTask;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            OpenFolderInLinuxFileManager(defaultPath);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Desktop only.
    /// </summary>
    /// <param name="folderPath">.</param>
    private static void OpenFolderInWindowsExplorer(string folderPath)
    {
        Process.Start(
            new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{folderPath}\"",
                UseShellExecute = true,
            }
        );
    }

    private static void OpenFolderInMacFinder(string folderPath)
    {
        Process.Start(
            new ProcessStartInfo
            {
                FileName = "open",
                Arguments = $"\"{folderPath}\"",
                UseShellExecute = true,
            }
        );
    }

    private static void OpenFolderInLinuxFileManager(string folderPath)
    {
        Process.Start(
            new ProcessStartInfo
            {
                FileName = "xdg-open",
                Arguments = $"\"{folderPath}\"",
                UseShellExecute = true,
            }
        );
    }
}
