using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.IO;

public sealed class FileInspectorPageViewModel : PacketInspectorPageViewModel
{
    public new const string PageId = "io.file-inspector";

    public FileInspectorPageViewModel(
        IPageContext context,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService extensions,
        PacketFileReader reader,
        IDeviceManager deviceManager,
        IThemeService themeService)
        : base(context, loggerFactory, dialogService, extensions, null, TimeProvider.System, deviceManager, themeService, PageId, reader) { }
}
