using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using Asv.Avalonia.IO;
using Asv.Cfg;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using NuGet.ContentModel;

namespace Asv.Avalonia.Example;

public interface IMavParamsPageViewModel : IDevicePage { }

[ExportPage(PageId)]
public class MavParamsPageViewModel : DevicePage<IMavParamsPageViewModel>, IMavParamsPageViewModel
{
    public const string PageId = "mav-params";
    public const MaterialIconKind PageIcon = MaterialIconKind.CogTransferOutline;

    public MavParamsPageViewModel()
        : this(devices: null!, NullCommandService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public MavParamsPageViewModel(IDeviceManager devices, ICommandService cmd)
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
