using Asv.IO;
using ObservableCollections;

namespace Asv.Avalonia.IO;

public class SettingsConnectionViewModel : RoutableViewModel, ISettingsSubPage
{
    private readonly ObservableList<PortViewModel> _portSource;

    public const string SubPageId = "settings.connection";

    public SettingsConnectionViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public SettingsConnectionViewModel(IDeviceManager deviceManager)
        : base(SubPageId)
    {
        _portSource = new ObservableList<PortViewModel>();
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        throw new NotImplementedException();
    }

    public IExportInfo Source => IoModule.Instance;

    public ValueTask Init(ISettingsPage context)
    {
        return ValueTask.CompletedTask;
    }
}

public class PortViewModel : RoutableViewModel
{
    public const string Id = "port";

    public PortViewModel(IProtocolPort port)
        : base(Id)
    {
        InitArgs(port.Id);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
