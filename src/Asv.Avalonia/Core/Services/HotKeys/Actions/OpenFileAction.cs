using Asv.Cfg;
using Avalonia.Input;
using Material.Icons;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public class FileCommandConfig
{
    public const int MaxRecentFiles = 10;

    public string? LastDirectory { get; set; }
    public List<string> RecentFiles { get; set; } = [];
}

public class OpenFileAction(
    IFileAssociationService files,
    IDialogService dialogs,
    IHostEnvironment path,
    IConfiguration config
) : HotKeyAction<IShell>
{
    public const string Id = "file.open";

    public override string ActionId => Id;
    public override string Name => RS.OpenFileCommand_CommandInfo_Name;
    public override string Description => RS.OpenFileCommand_CommandInfo_Description;
    public override MaterialIconKind Icon => MaterialIconKind.File;
    public override KeyGesture DefaultHotKey => new(Key.O, KeyModifiers.Control);

    protected override async ValueTask Execute(IShell target, CancellationToken cancel)
    {
        var dialog = dialogs.GetDialogPrefab<OpenFileDialogDesktopPrefab>();
        await OpenAsync(files, dialog, path, config, cancel);
    }

    public static async ValueTask OpenAsync(
        IFileAssociationService files,
        OpenFileDialogDesktopPrefab dialog,
        IHostEnvironment path,
        IConfiguration config,
        CancellationToken cancel = default
    )
    {
        if (cancel.IsCancellationRequested)
        {
            return;
        }

        var lastDirectory = path.ContentRootPath;
        var cfg = config.Get<FileCommandConfig>();
        if (cfg.LastDirectory != null && Directory.Exists(cfg.LastDirectory))
        {
            lastDirectory = cfg.LastDirectory;
        }

        var typeFilter = string.Join(
            ",",
            files.SupportedFiles.Select(x => x.Extension).Distinct().Append("*")
        );
        var filePath = await dialog.ShowDialogAsync(
            new OpenFileDialogPayload
            {
                Title = RS.OpenFileCommand_SelectFile,
                TypeFilter = typeFilter,
                InitialDirectory = lastDirectory,
            }
        );

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        await files.Open(filePath);
        AddRecentFile(config, filePath);
    }

    public static IReadOnlyList<string> GetRecentFiles(IConfiguration config)
    {
        var cfg = config.Get<FileCommandConfig>();
        return (cfg.RecentFiles ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(FileCommandConfig.MaxRecentFiles)
            .ToArray();
    }

    public static void AddRecentFile(IConfiguration config, string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        var fullPath = Path.GetFullPath(filePath);
        var cfg = config.Get<FileCommandConfig>();
        var recentFiles = (cfg.RecentFiles ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(Path.GetFullPath)
            .Where(x => !string.Equals(x, fullPath, StringComparison.OrdinalIgnoreCase))
            .Prepend(fullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(FileCommandConfig.MaxRecentFiles)
            .ToList();

        cfg.LastDirectory = Path.GetDirectoryName(fullPath);
        cfg.RecentFiles = recentFiles;
        config.Set(cfg);
    }
}
