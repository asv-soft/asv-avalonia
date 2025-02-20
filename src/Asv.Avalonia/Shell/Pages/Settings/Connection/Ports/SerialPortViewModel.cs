using System.Collections.ObjectModel;

namespace Asv.Avalonia;

public class SerialPortViewModel(string id) : RoutableViewModel(id)
{
    public override ValueTask<IRoutable> Navigate(string id)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        throw new NotImplementedException();
    }
}