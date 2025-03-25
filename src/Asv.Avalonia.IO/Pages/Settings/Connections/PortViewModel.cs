using Asv.Common;
using Asv.IO;
using Material.Icons;
using R3;

namespace Asv.Avalonia.IO;

public class PortViewModel : RoutableViewModel
{
    private MaterialIconKind? _icon;
    public const string ViewModelId = "port";

    public PortViewModel()
        : base(ViewModelId)
    {
        DesignTime.ThrowIfNotDesignMode();
        InitArgs(Guid.NewGuid().ToString());
        Icon = MaterialIconKind.Connection;
    }

    public PortViewModel(IProtocolPort port, IDeviceManager deviceManager)
        : base(ViewModelId)
    {
        InitArgs(port.Id);
        Icon = deviceManager.GetIcon(port.TypeInfo);
        port.Status.Subscribe(UpdateStatus).DisposeItWith(Disposable);
    }

    private void UpdateStatus(ProtocolPortStatus status)
    {
        switch (status)
        {
            case ProtocolPortStatus.Disconnected:

                break;
            case ProtocolPortStatus.InProgress:
                break;
            case ProtocolPortStatus.Connected:
                break;
            case ProtocolPortStatus.Error:
                IsError = true;
                IsSuccess = false;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }

    public bool IsError { get; set; }
    public bool IsSuccess { get; set; }
    public bool IsInProgress { get; set; }

    public MaterialIconKind? Icon
    {
        get => _icon;
        set => SetField(ref _icon, value);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
