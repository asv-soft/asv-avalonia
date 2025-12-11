namespace Asv.Avalonia.IO;

public class IoModuleOptionsBuilder
{
    private bool _isDeviceFeatureEnabled;

    internal IoModuleOptionsBuilder() { }

    internal IoModuleOptionsBuilder(IoModuleOptions defaultOptions)
    {
        _isDeviceFeatureEnabled = defaultOptions.IsDeviceFeatureEnabled;
    }

    public IoModuleOptionsBuilder WithDevices()
    {
        _isDeviceFeatureEnabled = true;
        return this;
    }

    public IoModuleOptions Build()
    {
        return new IoModuleOptions
        {
            IsEnabled = true,
            IsDeviceFeatureEnabled = _isDeviceFeatureEnabled,
        };
    }
}
