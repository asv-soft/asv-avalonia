using System.Windows.Input;
using Asv.IO;
using Asv.Modeling;

namespace Asv.Avalonia.IO;

public static class DevicePageViewModelMixin
{
    public const string ArgsDeviceIdKey = "dev_id";

    public static CommandArg CreateOpenPageArgs(DeviceId id)
    {
        return new StringArg(
            new NavArgs(new KeyValuePair<string, string?>(ArgsDeviceIdKey, id.AsString())).ToString()
        );
    }
}
