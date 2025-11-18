using Asv.IO;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.IO;

public class UdpEndpointViewModel : EndpointViewModel
{
    public UdpEndpointViewModel(
        IProtocolEndpoint protocolEndpoint,
        ILoggerFactory loggerFactory,
        TimeProvider timeProvider
    )
        : base(protocolEndpoint, loggerFactory, timeProvider)
    {
        Header = $"Address {((UdpSocketProtocolEndpoint)protocolEndpoint).RemoteEndPoint}";
    }
}
