using System.Composition;
using Asv.Cfg;
using Material.Icons;

namespace Asv.Avalonia;

public class FileCommandConfig
{
    public string? LastDirectory { get; set; }
}

[ExportCommand]
[Shared]
[method: ImportingConstructor]
public class OpenFileCommand(
    IFileAssociationService svc,
    IDialogService dialogs,
    IAppPath path,
    IConfiguration config
) : StatelessCommand<EmptyArg, DictArg>
{
    public const string DialogTitleArgKey = "title";
    public const string InitialDirectoryArgKey = "dir";
    public const string FilePathArgKey = "file";
    public const string TypeFiltersArgKey = "ext";

    #region Static

    public const string Id = $"{BaseId}.file.open";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.OpenFileCommand_CommandInfo_Name,
        Description = RS.OpenFileCommand_CommandInfo_Description,
        Icon = MaterialIconKind.File,
        DefaultHotKey = "Ctrl+O",
        Source = SystemModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    protected override bool InternalCanExecute(EmptyArg arg)
    {
        return true;
    }

    protected override bool InternalCanExecute(DictArg arg)
    {
        return true;
    }

    public static DictArg CreateArg(
        string? initialDirectory = null,
        string? filePath = null,
        string[]? typeFilters = null,
        string? title = null
    )
    {
        var arg = new DictArg();
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

        if (typeFilters != null)
        {
            arg[FilePathArgKey] = new ListArg(typeFilters.Select(x => new StringArg(x)));
        }
        return arg;
    }

    protected override async ValueTask<EmptyArg?> InternalExecute(
        EmptyArg arg,
        CancellationToken cancel
    )
    {
        await InternalExecute(new DictArg(), cancel);
        return null;
    }

    protected override async ValueTask<DictArg?> InternalExecute(
        DictArg arg,
        CancellationToken cancel
    )
    {
        var filePath =
            arg.TryGetValue(FilePathArgKey, out var filePathArg) && filePathArg is StringArg strArg
                ? strArg.Value
                : null;

        if (filePath == null)
        {
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

            var typeFilter = string.Join(
                ",",
                svc.SupportedFiles.Select(x => x.Extension).Distinct().Append("*")
            );
            if (arg.TryGetValue(TypeFiltersArgKey, out var filters) && filters is ListArg list)
            {
                var filterStrings = list.OfType<StringArg>()
                    .Select(x => x.Value)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray();
                if (filterStrings.Length > 0)
                {
                    typeFilter = string.Join(",", filterStrings.Append("*"));
                }
            }

            var dialogTitle = RS.OpenFileCommand_SelectFile;
            if (arg.TryGetValue(DialogTitleArgKey, out var title) && title is StringArg titleArg)
            {
                dialogTitle = titleArg.Value;
            }

            var dlg = dialogs.GetDialogPrefab<OpenFileDialogDesktopPrefab>();
            filePath = await dlg.ShowDialogAsync(
                new OpenFileDialogPayload
                {
                    Title = dialogTitle,
                    TypeFilter = typeFilter,
                    InitialDirectory = lastDirectory,
                }
            );
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return null;
        }
        config.Set(new FileCommandConfig { LastDirectory = Path.GetDirectoryName(filePath) });
        await svc.Open(filePath);
        return null;
    }
}
