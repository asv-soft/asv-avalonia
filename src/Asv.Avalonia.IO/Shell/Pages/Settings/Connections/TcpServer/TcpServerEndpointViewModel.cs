using Asv.IO;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.IO;

public class TcpServerEndpointViewModel : EndpointViewModel
{
    public TcpServerEndpointViewModel(
        IProtocolEndpoint protocolEndpoint,
        ILoggerFactory loggerFactory,
        TimeProvider timeProvider
    )
        : base(protocolEndpoint, loggerFactory, timeProvider)
    {
        Header =
            $"Address {((TcpServerSocketProtocolEndpoint)protocolEndpoint).Socket.RemoteEndPoint}";
    }
}
