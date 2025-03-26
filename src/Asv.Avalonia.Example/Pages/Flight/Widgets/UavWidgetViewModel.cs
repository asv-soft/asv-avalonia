using System.Collections.Generic;
using Asv.Common;
using Asv.IO;
using R3;

namespace Asv.Avalonia.Example;

public class UavWidgetViewModel : ExtendableHeadlinedViewModel<IUavFlightWidget>, IUavFlightWidget
{
    private WorkspaceDock _position;
    public const string WidgetId = "widget-uav";

    public UavWidgetViewModel()
        : base(WidgetId)
    {
        DesignTime.ThrowIfNotDesignMode();
        InitArgs("1");
    }

    public UavWidgetViewModel(IClientDevice device)
        : base(WidgetId)
    {
        Device = device;
        Position = WorkspaceDock.Left;
        Icon = DeviceIconMixin.GetIcon(device.Id);
        IconBrush = DeviceIconMixin.GetIconBrush(device.Id);
        device.Name.Subscribe(x => Header = x).DisposeItWith(Disposable);
        InitArgs(device.Id.AsString());
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }

    public IClientDevice Device { get; }

    public WorkspaceDock Position
    {
        get => _position;
        set => SetField(ref _position, value);
    }
}
