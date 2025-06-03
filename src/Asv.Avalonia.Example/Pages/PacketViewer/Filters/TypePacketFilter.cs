using System.Collections.Generic;
using Asv.Common;
using R3;

namespace Asv.Avalonia.Example;

public sealed class TypePacketFilter : PacketFilterBase<TypePacketFilter>
{
    public TypePacketFilter(PacketMessageViewModel pkt, IUnitService unitService)
        : base(unitService)
    {
        Value = new BindableReactiveProperty<string>(pkt.Type).DisposeItWith(Disposable);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public override required BindableReactiveProperty<string> Value { get; init; }
}
