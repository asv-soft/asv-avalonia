using System.ComponentModel.DataAnnotations;
using Asv.Mavlink;

namespace Asv.Avalonia.Example;

public interface IFtpService : IExportable
{
    [Range(1, MavlinkFtpHelper.MaxDataSize)]
    public byte BurstDownloadPacketSize { get; set; }
}
