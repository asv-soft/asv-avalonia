using System.Diagnostics;
using System.Runtime.InteropServices;
using R3;

namespace Asv.Avalonia;

/// <summary>
/// Payload for ObserveFolderDialog prefab.
/// </summary>
public sealed class ObserveFolderDialogPayload
{
    /// <summary>
    /// Gets or inits the title of the dialog.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets or inits the default path.
    /// </summary>
    public required string? DefaultPath { get; init; }
}

/// <summary>
/// Dialog to observe a folder.
/// </summary>
public sealed class ObserveFolderDialogPrefab : IDialogPrefab<ObserveFolderDialogPayload, Unit>
{
    public Task<Unit> ShowDialogAsync(ObserveFolderDialogPayload dialogPayload)
    {
        if (dialogPayload.DefaultPath is null)
        {
            return Task.FromResult(Unit.Default);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            OpenFolderInWindowsExplorer(dialogPayload.DefaultPath);
            return Task.FromResult(Unit.Default);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            OpenFolderInMacFinder(dialogPayload.DefaultPath);
            return Task.FromResult(Unit.Default);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            OpenFolderInLinuxFileManager(dialogPayload.DefaultPath);
        }

        return Task.FromResult(Unit.Default);
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
