namespace Asv.Avalonia.IO;

public class IoModuleOptionsBuilder
{
    private bool _enableDevices;

    internal IoModuleOptionsBuilder() { }

    internal IoModuleOptionsBuilder(IoModuleOptions defaultOptions)
    {
        _enableDevices = defaultOptions.EnableDevices;
    }

    public IoModuleOptionsBuilder WithDevices()
    {
        _enableDevices = true;
        return this;
    }

    public IoModuleOptions Build()
    {
        return new IoModuleOptions { EnableDevices = _enableDevices };
    }
}
