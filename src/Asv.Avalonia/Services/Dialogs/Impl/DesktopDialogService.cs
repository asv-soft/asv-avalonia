using System.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using R3;

namespace Asv.Avalonia;

[Export(typeof(IDialogService))]
[Export(typeof(ISimpleDialogService))]
[Shared]
public sealed class DesktopDialogService : IDialogService, ISimpleDialogService
{
    private readonly IShellHost _host;

    [ImportingConstructor]
    public DesktopDialogService(IShellHost host)
    {
        _host = host;
    }

    public bool IsImplementedShowOpenFileDialog { get; } = true;
    public bool IsImplementedShowSaveFileDialog { get; } = true;
    public bool IsImplementedShowSelectFolderDialog { get; } = true;
    public bool IsImplementedShowObserveFolderDialog { get; } = true;
    public bool IsImplementedShowYesNoDialogDialog { get; } = true;
    public bool IsImplementedShowSaveCancelDialog { get; } = true;
    public bool IsImplementedShowUnitInputDialog { get; } = true;

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

    public async Task<bool> ShowYesNoDialog(string title, string message)
    {
        var result = await CustomDialogInterface.ShowCustomDialog(_host.TopLevel as Window, title, message, false, "Yes", "Yes", "No");
        return result != null && (string)result == "Yes";
    }

    public async Task<bool> ShowSaveCancelDialog(string title, string message)
    {
        var result = await CustomDialogInterface.ShowCustomDialog(_host.TopLevel as Window, title, message, false, "Save", "Save", "Cancel");
        return result != null && (string)result == "Save";
    }

    public async Task<string?> ShowUnitInputDialog(string title, string message)
    {
        var result = await CustomDialogInterface.ShowCustomDialog(_host.TopLevel as Window, title, message, true, "OK", "OK", "Cancel");
        return result as string;
    }

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
