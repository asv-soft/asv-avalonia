using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;

namespace Asv.Avalonia.Example;

public interface IFlightModeContext : IPage { }

[ExportPage(PageId)]
public class FlightPageViewModel : PageViewModel<IFlightModeContext>, IFlightModeContext
{
    public const string PageId = "Flight";

    public FlightPageViewModel()
        : this(DesignTime.CommandService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public FlightPageViewModel(ICommandService cmd)
        : base(PageId, cmd)
    {
        Title.Value = "Flight";
    }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        throw new System.NotImplementedException();
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        throw new System.NotImplementedException();
    }

    protected override void AfterLoadExtensions()
    {
        throw new System.NotImplementedException();
    }

    public override IExportInfo Source { get; }
}
