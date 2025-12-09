using Asv.IO;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.IO;

public class TcpServerEndpointViewModel : EndpointViewModel
{
    public TcpServerEndpointViewModel(
        IProtocolEndpoint protocolEndpoint,
        IUnitService unitService,
        ILoggerFactory loggerFactory,
        TimeProvider timeProvider
    )
        : base(protocolEndpoint, unitService, loggerFactory, timeProvider)
    {
        Header =
            $"{RS.TcpServerEndpointViewModel_Header} {((TcpServerSocketProtocolEndpoint)protocolEndpoint).Socket.RemoteEndPoint}";
    }
}
