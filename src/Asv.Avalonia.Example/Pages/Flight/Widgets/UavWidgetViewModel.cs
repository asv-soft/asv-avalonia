using System.Collections.Generic;
using Asv.IO;

namespace Asv.Avalonia.Example;

public interface IUavFlightWidget
{
    IClientDevice Device { get; }
}

public class UavWidgetViewModel : ExtendableViewModel<IUavFlightWidget>, IUavFlightWidget
{
    public UavWidgetViewModel(IClientDevice device)
        : base("widget")
    {
        Device = device;
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        throw new System.NotImplementedException();
    }

    protected override void AfterLoadExtensions()
    {
        throw new System.NotImplementedException();
    }

    public IClientDevice Device { get; }
}
