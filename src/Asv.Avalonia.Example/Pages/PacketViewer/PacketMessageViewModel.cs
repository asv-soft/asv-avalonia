using System;
using System.Collections.Generic;
using System.Linq;
using Asv.Avalonia.Example.Converters;
using Asv.IO;
using Asv.Mavlink;
using Avalonia;
using ObservableCollections;
using R3;
using PacketFormatting = Asv.Avalonia.Example.Converters.PacketFormatting;

namespace Asv.Avalonia.Example;

public class PacketMessageViewModel : RoutableViewModel
{
    public const string PageId = "packet-message";
    public DateTime DateTime { get; set; }
    public string Source { get; set; }
    public int Size { get; set; }
    public string Message { get; set; }
    public string Type { get; set; }

    private bool _highlight;

    public bool Highlight
    {
        get => _highlight;
        set => SetField(ref _highlight, value);
    }

    public PacketMessageViewModel()
        : base(NavigationId.Empty)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public PacketMessageViewModel(MavlinkMessage packet, IPacketConverter converter)
        : base($"{PageId}.{packet.Id.ToString()}")
    {
        Id.ChangeArgs(Guid.NewGuid().ToString());
        DateTime = DateTime.Now;
        Source = $"[{packet.SystemId},{packet.ComponentId}]";
        Message = $"[{packet.Sequence:000}] {converter.Convert(packet)}";
        Description = converter.Convert(packet, PacketFormatting.Indented);
        Type = packet.Name;
        Size = packet.GetByteSize();
    }

    public string Description { get; }

    public bool Match(Func<PacketMessageViewModel, bool> condition)
    {
        return condition(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
