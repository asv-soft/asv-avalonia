using Asv.Cfg;
using Avalonia.Input;
using Material.Icons;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public class FileCommandConfig
{
    public string? LastDirectory { get; set; }
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

    protected override async ValueTask<bool> Execute(IShell target, CancellationToken cancel)
    {
        var dialog = dialogs.GetDialogPrefab<OpenFileDialogDesktopPrefab>();
        await OpenAsync(files, dialog, path, config, cancel);
        return true;
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

        config.Set(new FileCommandConfig { LastDirectory = Path.GetDirectoryName(filePath) });
        await files.Open(filePath);
    }
}
