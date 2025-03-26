using System;
using System.Composition;
using Asv.Avalonia.IO;
using Asv.Avalonia.Map;
using Asv.Cfg;
using Asv.IO;
using Asv.Mavlink;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Avalonia.Example;

public class MavlinkProtocolProviderConfig
{
    public byte SystemId { get; set; } = 255;
    public byte ComponentId { get; set; } = 255;
}

[Export(typeof(IDeviceManagerExtension))]
[Shared]
public class MavlinkProtocolProvider : IDeviceManagerExtension
{
    private readonly IConfiguration _cfgService;
    private readonly IPacketSequenceCalculator _seq;
    private readonly MavlinkProtocolProviderConfig _config;

    [ImportingConstructor]
    public MavlinkProtocolProvider(IConfiguration cfgService, IPacketSequenceCalculator seq)
    {
        ArgumentNullException.ThrowIfNull(cfgService);
        ArgumentNullException.ThrowIfNull(seq);
        _cfgService = cfgService;
        _seq = seq;
        _config = cfgService.Get<MavlinkProtocolProviderConfig>();
    }

    public void Configure(IProtocolBuilder builder)
    {
        builder.RegisterMavlinkV2Protocol();
        builder.Features.RegisterMavlinkV2WrapFeature();
    }

    public void Configure(IDeviceExplorerBuilder builder)
    {
        builder.Factories.RegisterDefaultDevices(
            new MavlinkIdentity(_config.SystemId, _config.ComponentId),
            _seq,
            _cfgService
        );
    }

    public bool TryGetIcon(DeviceId id, out MaterialIconKind? icon)
    {
        icon = DeviceIconMixin.GetIcon(id);
        return icon != null;
    }

    public bool TryGetDeviceBrush(DeviceId id, out IBrush? brush)
    {
        brush = null;
        return false;
    }
}
