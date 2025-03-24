using Asv.IO;

namespace Asv.Avalonia.IO;

public class PortViewModel : RoutableViewModel
{
    public const string ViewModelId = "port";

    public PortViewModel() 
        : base(ViewModelId)
    {
        DesignTime.ThrowIfNotDesignMode();
        InitArgs(Guid.NewGuid().ToString());
    }
    public PortViewModel(IProtocolPort port)
        : base(ViewModelId)
    {
        InitArgs(port.Id);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}