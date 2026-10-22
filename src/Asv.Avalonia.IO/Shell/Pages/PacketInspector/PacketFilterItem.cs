using System.Collections.ObjectModel;

namespace Asv.Avalonia.IO;

public sealed class PacketFilterItem(string key, string name, Action changed) : ViewModel("packet-filter")
{
    private bool? _isChecked = true;
    public string Key { get; } = key;
    private string _name = name;
    private string? _description;
    public string Name => _name;
    public string? Description => _description;

    internal void UpdatePresentation(string name, string? description)
    {
        SetField(ref _name, name, nameof(Name));
        SetField(ref _description, description, nameof(Description));
    }
    public PacketDevicePresentation? DevicePresentation { get; init; }
    public ObservableCollection<PacketFilterItem> Children { get; } = [];
    public bool? IsChecked
    {
        get => _isChecked;
        set
        {
            if (_isChecked == value)
            {
                return;
            }
            SetField(ref _isChecked, value);
            if (value.HasValue)
            {
                foreach (var child in Children)
                {
                    child.SetChecked(value.Value);
                }
            }
            changed();
        }
    }

    internal void SetChecked(bool value)
    {
        SetField(ref _isChecked, value, nameof(IsChecked));
        foreach (var child in Children)
        {
            child.SetChecked(value);
        }
    }

    internal void UpdateParent()
    {
        if (Children.Count > 0)
        {
            bool? value = Children.All(x => x.IsChecked == true) ? true
                : Children.All(x => x.IsChecked == false) ? false : null;
            SetField(ref _isChecked, value, nameof(IsChecked));
        }
    }
}

public sealed class PacketMessageRow(PacketSnapshot packet, int count, double frequency) : ViewModel("packet-message")
{
    public PacketSnapshot Packet { get; private set; } = packet;
    public PacketDevicePresentation? DevicePresentation { get; init; }
    public int Count { get; private set => SetField(ref field, value); } = count;
    public double Frequency { get; private set; } = frequency;
    public string Name => Packet.Name;
    public string Protocol => Packet.Protocol;
    public string Endpoint => string.IsNullOrWhiteSpace(Packet.EndpointName) ? Packet.Endpoint : Packet.EndpointName;
    public string Device => Packet.Device;
    public string Direction => Packet.Direction;
    public string MessageId => Packet.MessageId;
    public string Rate { get; private set => SetField(ref field, value); } = frequency.ToString("0.0");

    internal void Update(PacketSnapshot latest, int total, double rate)
    {
        Packet = latest;
        Count = total;
        Frequency = rate;
        Rate = rate.ToString("0.0");
    }
}
