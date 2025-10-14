using Asv.IO;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.IO;

public class TcpServerEndpointViewModel : EndpointViewModel
{
    public TcpServerEndpointViewModel(
        IProtocolEndpoint protocolEndpoint,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        TimeProvider timeProvider
    )
        : base(protocolEndpoint, layoutService, loggerFactory, timeProvider)
    {
        Header =
            $"Address {((TcpServerSocketProtocolEndpoint)protocolEndpoint).Socket.RemoteEndPoint}";
    }
}
