using System;
using Asv.Avalonia.Example.Converters;
using Asv.IO;
using Asv.Mavlink;
using Avalonia;
using R3;
using PacketFormatting = Asv.Avalonia.Example.Converters.PacketFormatting;

namespace Asv.Avalonia.Example.PacketViewer;

public class PacketMessageViewModel : AvaloniaObject
{
    public DateTime DateTime { get; set; }
    public string Source { get; set; }
    public int Size { get; set; }
    public string Message { get; set; }
    public string Type { get; set; }

    private bool _highlight;

    public static readonly DirectProperty<PacketMessageViewModel, bool> HighlightProperty =
        AvaloniaProperty.RegisterDirect<PacketMessageViewModel, bool>(
            nameof(Highlight),
            o => o.Highlight,
            (o, v) => o.Highlight = v
        );

    public bool Highlight
    {
        get => _highlight;
        set => SetAndRaise(HighlightProperty, ref _highlight, value);
    }

    public PacketMessageViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public PacketMessageViewModel(MavlinkMessage packet, IPacketConverter converter)
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
}
