using System.Linq;
using Asv.Avalonia.Map;
using Asv.IO;
using Asv.Mavlink;

namespace Asv.Avalonia.Example;

public class UavAnchor : MapAnchor
{
    public UavAnchor()
        : base("uav_design_time")
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public UavAnchor(IClientDevice uav)
        : base($"uav_{uav.Id.AsString()}") { }
}
