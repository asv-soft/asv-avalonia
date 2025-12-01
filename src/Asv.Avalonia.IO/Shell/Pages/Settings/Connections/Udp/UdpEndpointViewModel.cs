using Asv.IO;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.IO;

public class UdpEndpointViewModel : EndpointViewModel
{
    public UdpEndpointViewModel(
        IProtocolEndpoint protocolEndpoint,
        IUnitService unitService,
        ILoggerFactory loggerFactory,
        TimeProvider timeProvider
    )
        : base(protocolEndpoint, unitService, loggerFactory, timeProvider)
    {
        Header =
            $"{RS.UdpEndpointViewModel_Header} {((UdpSocketProtocolEndpoint)protocolEndpoint).RemoteEndPoint}";
    }
}
