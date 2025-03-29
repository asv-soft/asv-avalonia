using System.Composition;

namespace Asv.Avalonia.Example;

[Export(typeof(IFtpService))]
[Shared]
public class FtpService : IFtpService
{
    public byte BurstDownloadPacketSize { get; set; }
    public IExportInfo Source => SystemModule.Instance;
}
