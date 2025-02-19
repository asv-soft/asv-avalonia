using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;

namespace Asv.Avalonia.Example;

[ExportPage(PageId)]
public class FlightModePageViewModel : PageViewModel<FlightModePageViewModel>
{
    public const string PageId = "flight.mode";

    public FlightModePageViewModel()
        : this(DesignTime.CommandService)
    {
        DesignTime.ThrowIfNotDesignMode();
        Title.OnNext(RS.FlightPageViewModel_Title);
    }

    [ImportingConstructor]
    public FlightModePageViewModel(ICommandService cmd)
        : base(PageId, cmd)
    {
        Title.OnNext(RS.FlightPageViewModel_Title);
    }

    protected override FlightModePageViewModel GetContext()
    {
        return this;
    }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions() { }

    public override IExportInfo Source => SystemModule.Instance;
}
