using System;
using System.Collections.Generic;
using Asv.Avalonia.Example.Converters;
using Asv.IO;
using Asv.Mavlink;
using Avalonia;
using R3;
using PacketFormatting = Asv.Avalonia.Example.Converters.PacketFormatting;

namespace Asv.Avalonia.Example.PacketViewer;

public class PacketMessageViewModel : RoutableViewModel
{
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
        : base(packet.Id.ToString())
    {
        DateTime = DateTime.Now;
        Source = $"[{packet.SystemId},{packet.ComponentId}]";
        Message = $"[{packet.Sequence:000}] {converter.Convert(packet)}";
        Description = converter.Convert(packet, PacketFormatting.Indented);
        Type = packet.Name;
        Id = Guid.NewGuid();
        Size = packet.GetByteSize();
    }

    public Guid Id { get; }
    public string Description { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
