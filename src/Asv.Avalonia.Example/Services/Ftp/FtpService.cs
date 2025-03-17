using System.Composition;
using Asv.Mavlink;

namespace Asv.Avalonia.Example;

[Export(typeof(IFtpService))]
[Shared]
public class FtpService : IFtpService
{
    public IFtpClient? Client { get; set; }
    public IExportInfo Source => SystemModule.Instance;
}
