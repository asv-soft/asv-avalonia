using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.IO;

public sealed class FileInspectorPageViewModel : PacketInspectorPageViewModel
{
    public new const string PageId = "io.file-inspector";
    public new const AsvColorKind PageIconColor = AsvColorKind.Info5;

    public FileInspectorPageViewModel(
        IPageContext context,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService extensions,
        PacketFileReader reader,
        IDeviceManager deviceManager,
        IThemeService themeService)
        : base(context, loggerFactory, dialogService, extensions, null, TimeProvider.System, deviceManager, themeService, PageId, reader)
    {
        IconColor = PageIconColor;
        ExportWidget = new PacketInspectorWidget("packet-file-export", FileExportStrings.Title, WorkspaceDock.Left,
            MaterialIconKind.Export, this).DisposeItWith(Disposable);
        Export = new PacketFileExportViewModel(new PacketFileExporter(deviceManager.ProtocolFactory), dialogService).DisposeItWith(Disposable);
        Export.SetParent(this);
        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(HasFile) or nameof(FilePath) or nameof(IsLoading))
            {
                Export.SetSource(HasFile && !IsLoading ? FilePath : null);
            }
        };
    }

    public PacketInspectorWidget ExportWidget { get; }
    public PacketFileExportViewModel Export { get; }

    public override IEnumerable<IViewModel> GetChildren() => base.GetChildren().Concat([ExportWidget, Export]);
}
