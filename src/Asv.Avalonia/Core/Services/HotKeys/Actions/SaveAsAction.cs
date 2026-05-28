using Asv.Cfg;
using Avalonia.Input;
using Material.Icons;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public class SaveAsAction(IDialogService dialogs, IHostEnvironment path, IConfiguration config)
    : HotKeyAction<ISupportSaveAs>
{
    public const string Id = "save_as";
    public override string ActionId => Id;
    public override string Name => RS.SaveAsCommand_CommandInfo_Name;
    public override string Description => RS.SaveAsCommand_CommandInfo_Description;
    public override MaterialIconKind Icon => MaterialIconKind.FloppyDisc;
    public override KeyGesture DefaultHotKey =>
        new(Key.S, KeyModifiers.Control | KeyModifiers.Shift);

    protected override async ValueTask InternalExecute(
        ISupportSaveAs target,
        CancellationToken cancel
    )
    {
        if (cancel.IsCancellationRequested)
        {
            return;
        }

        var dialog = dialogs.GetDialogPrefab<SaveFileDialogDesktopPrefab>();
        var filePath = await dialog.ShowDialogAsync(
            new SaveFileDialogPayload
            {
                Title = Name,
                DefaultExt = target.DefaultExtension,
                SuggestedFileName = target.DefaultFileName,
                TypeFilter = target.TypeFilter,
                InitialDirectory = GetInitialDirectory(target),
            }
        );

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        await target.SaveAs(filePath, cancel);
        OpenFileAction.AddRecentFile(config, filePath);
    }

    private string GetInitialDirectory(ISupportSaveAs target)
    {
        if (
            target.CurrentFilePath is { } currentFilePath
            && Path.GetDirectoryName(currentFilePath) is { } currentDirectory
            && Directory.Exists(currentDirectory)
        )
        {
            return currentDirectory;
        }

        var cfg = config.Get<FileCommandConfig>();
        if (cfg.LastDirectory != null && Directory.Exists(cfg.LastDirectory))
        {
            return cfg.LastDirectory;
        }

        return path.ContentRootPath;
    }
}
