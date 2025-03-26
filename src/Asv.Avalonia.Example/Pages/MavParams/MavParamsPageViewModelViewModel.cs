using System.Collections.Generic;
using System.Composition;
using Asv.Avalonia.IO;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Avalonia.Example;

public interface IMavParamsPageViewModel : IDevicePage { }

[ExportPage(PageId)]
public class MavParamsPageViewModelViewModel
    : DevicePageViewModel<IMavParamsPageViewModel>,
        IMavParamsPageViewModel
{
    public const string PageId = "mav-params";
    public const MaterialIconKind PageIcon = MaterialIconKind.CogTransferOutline;

    public MavParamsPageViewModelViewModel()
        : this(devices: null!, NullCommandService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public MavParamsPageViewModelViewModel(IDeviceManager devices, ICommandService cmd)
        : base(PageId, devices, cmd)
    {
        Title.Value = "Params";
        Icon.Value = PageIcon;
    }

    protected override void AfterDeviceInitialized(IClientDevice device)
    {
        Title.Value = $"Params[{device.Id}]";
        Params = device.GetMicroservice<IParamsClientEx>();
    }

    public IParamsClientEx? Params { get; private set; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }

    public override IExportInfo Source => SystemModule.Instance;
}
