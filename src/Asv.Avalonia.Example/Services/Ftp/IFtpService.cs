using Asv.Mavlink;

namespace Asv.Avalonia.Example;

public interface IFtpService : IExportable
{
    public IFtpClient? Client { get; set; }
}
