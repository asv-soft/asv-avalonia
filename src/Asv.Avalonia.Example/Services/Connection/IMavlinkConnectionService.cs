using System.Threading.Tasks;
using Asv.IO;
using ObservableCollections;

namespace Asv.Avalonia.Example;

public class ConnectionPortConfigItem
{
    public string ConnectionString { get; set; }
    public bool IsEnabled { get; set; }
}

public interface IMavlinkConnectionService : IExportable
{
    public IProtocolRouter Router { get; set; }
    public IDeviceExplorer DevicesExplorer { get; set; }
    public ObservableDictionary<string, IProtocolPort> Connections { get; set; }
    public void DisablePort(IProtocolPort port);
    public void EnablePort(IProtocolPort port);
    public Task RemovePort(IProtocolPort port, bool withDialog = true);
    public void AddConnection(string name, string connectionString, bool isEnabled = true);
    public ValueTask EditPort(IProtocolPort port);
}
