using System.Collections.Generic;
using Asv.Common;
using R3;

namespace Asv.Avalonia.Example;

public sealed class TypePacketFilterViewModel : PacketFilterViewModelBase<TypePacketFilterViewModel>
{
    public TypePacketFilterViewModel(PacketMessageViewModel pkt, IUnitService unitService)
        : base(unitService)
    {
        FilterValue = new BindableReactiveProperty<string>(pkt.Type).DisposeItWith(Disposable);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public override BindableReactiveProperty<string> FilterValue { get; }
}
