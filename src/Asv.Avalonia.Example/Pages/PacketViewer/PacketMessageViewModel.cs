using System;
using System.Collections.Generic;
using System.Linq;
using Asv.Mavlink;
using ObservableCollections;

namespace Asv.Avalonia.Example;

public class PacketMessageViewModel : RoutableViewModel
{
    public const string PageId = "packet-message";

    public DateTime DateTime { get; }
    public string Source { get; }
    public int Size { get; }
    public string Message { get; }
    public string Type { get; }
    public string Description { get; }

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
        DateTime = DateTime.Now;
        Source = "[1,1]";
        Message = "[1000] information";
        Description = "Some description";
        Type = "HEARTBEAT";
        Size = 10;
    }

    public PacketMessageViewModel(MavlinkMessage packet, IPacketConverter converter)
        : base(new NavigationId($"{PageId}.{packet.Id.ToString()}", Guid.NewGuid().ToString()))
    {
        DateTime = DateTime.Now;
        Source = $"[{packet.SystemId},{packet.ComponentId}]";
        Message = $"[{packet.Sequence:000}] {converter.Convert(packet)}";
        Description = converter.Convert(packet, PacketFormatting.Indented);
        Type = packet.Name;
        Size = packet.GetByteSize();
    }

    public bool Filter(
        string searchText,
        IEnumerable<TypePacketFilterViewModel> typeFilters,
        IEnumerable<SourcePacketFilterViewModel> sourceFilters
    )
    {
        var hasRequiredType = typeFilters.Any(f =>
            f.IsChecked.Value && f.FilterValue.Value == Type
        );

        var hasRequiredSource = sourceFilters.Any(f =>
            f.IsChecked.Value && f.FilterValue.Value == Source
        );

        if (!hasRequiredSource)
        {
            return false;
        }

        if (!hasRequiredType)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(searchText))
        {
            return true;
        }

        return Message.Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
