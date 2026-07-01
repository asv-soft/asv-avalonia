using Avalonia.Controls;
using Microsoft.Extensions.Options;

namespace Asv.Avalonia;

public class SoloRunFeatureBuilder
{
    private string? _mutexName;
    private bool? _argumentForwarding;
    private string? _pipeName;

    internal SoloRunFeatureBuilder() { }

    public SoloRunFeatureBuilder UseDefault()
    {
        return this;
    }

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

    internal OptionsBuilder<SoloRunFeatureOptions> Build(
        OptionsBuilder<SoloRunFeatureOptions> options
    )
    {
        return options.Configure(config =>
        {
            if (_mutexName is not null)
            {
                config.Mutex = _mutexName;
            }

            if (_argumentForwarding.HasValue)
            {
                config.ArgForward = _argumentForwarding.Value;
            }

            if (_pipeName is not null)
            {
                config.Pipe = _pipeName;
            }
        });
    }
}
