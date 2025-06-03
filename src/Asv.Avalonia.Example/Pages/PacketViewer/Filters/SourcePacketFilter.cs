using System.Collections.Generic;
using Asv.Common;
using R3;

namespace Asv.Avalonia.Example;

public sealed class SourcePacketFilter : PacketFilterBase<SourcePacketFilter>
{
    public SourcePacketFilter(PacketMessageViewModel pkt, IUnitService unitService)
        : base(unitService)
    {
        Value = new BindableReactiveProperty<string>(pkt.Source).DisposeItWith(Disposable);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public override required BindableReactiveProperty<string> Value { get; init; }
}
