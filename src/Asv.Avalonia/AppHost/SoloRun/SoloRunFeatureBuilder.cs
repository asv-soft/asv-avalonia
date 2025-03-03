using Microsoft.Extensions.Options;

namespace Asv.Avalonia;

public class SoloRunFeatureBuilder
{
    private string? _mutexName;
    private bool _argumentForwarding;
    private string? _pipeName;

    internal SoloRunFeatureBuilder() { }

    public SoloRunFeatureBuilder WithMutexName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        _mutexName = name;
        return this;
    }

    public SoloRunFeatureBuilder WithArgumentForwarding()
    {
        _argumentForwarding = true;
        return this;
    }

    public SoloRunFeatureBuilder WithArgumentForwarding(string pipeName)
    {
        ArgumentNullException.ThrowIfNull(pipeName);
        _argumentForwarding = true;
        _pipeName = pipeName;
        return this;
    }

    internal OptionsBuilder<SoloRunFeatureConfig> Build(
        OptionsBuilder<SoloRunFeatureConfig> options
    )
    {
        return options.Configure(config =>
        {
            config.Mutex = _mutexName;
            config.ArgForward = _argumentForwarding;
            config.Pipe = _pipeName;
        });
    }
}
