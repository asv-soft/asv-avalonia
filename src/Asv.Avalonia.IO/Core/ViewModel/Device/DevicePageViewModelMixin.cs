using Asv.IO;
using Asv.Modeling;

namespace Asv.Avalonia.IO;

public static class DevicePageViewModelMixin
{
    public const string ArgsDeviceIdKey = "dev_id";

    public static NavArgs CreateOpenPageArgs(DeviceId id)
    {
        return new NavArgs(new KeyValuePair<string, string?>(ArgsDeviceIdKey, id.AsString()));
    }
}
