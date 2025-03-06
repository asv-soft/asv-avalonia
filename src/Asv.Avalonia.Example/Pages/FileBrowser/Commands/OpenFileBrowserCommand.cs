using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.IO;
using Asv.Mavlink;

namespace Asv.Avalonia.Example;

[ExportCommand]
[method: ImportingConstructor]
public class OpenFileBrowserCommand(IFtpService svc) : ContextCommand<IShell>
{
    #region Static

    public const string Id = $"{BaseId}.open.file.browser";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "File browser",
        Description = "Open FTP file browser",
        Icon = FileBrowserViewModel.PageIcon,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion

    protected override async ValueTask<IPersistable?> InternalExecute(
        IShell context,
        IPersistable newValue,
        CancellationToken cancel
    )
    {
        if (newValue is not Persistable<IClientDevice> memento)
        {
            return null;
        }

        var client = memento.Value.GetMicroservice<IFtpClient>();
        svc.Client = client;
        await context.Navigate(FileBrowserViewModel.PageId);

        return null;
    }

    public override ICommandInfo Info => StaticInfo;
}
