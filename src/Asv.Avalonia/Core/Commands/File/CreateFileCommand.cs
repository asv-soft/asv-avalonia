using System.Composition;
using Asv.Cfg;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
[method: ImportingConstructor]
public class CreateFileCommand(
    IFileAssociationService svc,
    IDialogService dialogs,
    IAppPath path,
    IConfiguration config
) : StatelessCommand<DictArg>
{
    #region Static

    public const string DialogTitleArgKey = "title";
    public const string InitialDirectoryArgKey = "dir";
    public const string FilePathArgKey = "file";
    public const string FileTypeIdArgKey = "type";

    public const string Id = $"{BaseId}.file.create";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.CreateFileCommand_CommandInfo_Name,
        Description = RS.CreateFileCommand_CommandInfo_Description,
        Icon = MaterialIconKind.FilePlus,
        DefaultHotKey = "Ctrl+N",
        Source = SystemModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    public static DictArg CreateArg(
        FileTypeInfo fileInfo,
        string? initialDirectory = null,
        string? filePath = null,
        string? title = null
    )
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        var arg = new DictArg { [FileTypeIdArgKey] = new StringArg(fileInfo.Id) };
        if (title != null)
        {
            arg[DialogTitleArgKey] = new StringArg(title);
        }
        if (initialDirectory != null)
        {
            arg[InitialDirectoryArgKey] = new StringArg(initialDirectory);
        }

        if (filePath != null)
        {
            arg[FilePathArgKey] = new StringArg(filePath);
        }

        return arg;
    }

    protected override bool InternalCanExecute(DictArg arg)
    {
        return arg.TryGetValue(FileTypeIdArgKey, out var fileTypeId) && fileTypeId is StringArg;
    }

    protected override async ValueTask<DictArg?> InternalExecute(
        DictArg arg,
        CancellationToken cancel
    )
    {
        var fileTypeId =
            arg.TryGetValue(FileTypeIdArgKey, out var fileTypeIdArg)
            && fileTypeIdArg is StringArg fileTypeArg
                ? fileTypeArg.Value
                : null;
        if (fileTypeId == null)
        {
            throw new InvalidOperationException("File type id is not specified");
        }

        var fileType = svc.SupportedFiles.FirstOrDefault(x => x.Id == fileTypeId);
        if (fileType == null)
        {
            throw new InvalidOperationException($"File type {fileTypeId} is not supported");
        }

        string? filePath = null;

        if (
            arg.TryGetValue(FilePathArgKey, out var filePathArg)
            && filePathArg is StringArg filePathStringArg
        )
        {
            filePath = filePathStringArg.Value;
        }

        if (filePath == null)
        {
            var dialogTitle = RS.CreateFileCommand_SelectFile;
            if (arg.TryGetValue(DialogTitleArgKey, out var title) && title is StringArg titleArg)
            {
                dialogTitle = titleArg.Value;
            }
            var lastDirectory = path.UserDataFolder;
            var cfg = config.Get<FileCommandConfig>();
            if (cfg.LastDirectory != null && Directory.Exists(cfg.LastDirectory))
            {
                lastDirectory = cfg.LastDirectory;
            }
            if (
                arg.TryGetValue(InitialDirectoryArgKey, out var initialDirectory)
                && initialDirectory is StringArg dir
            )
            {
                lastDirectory = dir.Value;
            }
            var dlg = dialogs.GetDialogPrefab<SaveFileDialogDesktopPrefab>();
            filePath = await dlg.ShowDialogAsync(
                new SaveFileDialogPayload
                {
                    Title = dialogTitle,
                    TypeFilter = fileType.Extension,
                    InitialDirectory = lastDirectory,
                }
            );
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return null;
        }

        await svc.Create(filePath, fileType);
        return null;
    }
}
