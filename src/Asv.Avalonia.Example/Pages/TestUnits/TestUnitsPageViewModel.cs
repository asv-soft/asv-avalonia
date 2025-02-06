using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using R3;

namespace Asv.Avalonia.Example;

[ExportPage(PageId)]
public class TestUnitsPageViewModel : PageViewModel<TestUnitsPageViewModel>
{
    public const string PageId = "testUnits";

    private readonly IUnitService _unitService;
    private ReactiveProperty<double> _speed;

    public TestUnitsPageViewModel()
        : this(DesignTime.UnitService, DesignTime.CommandService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public TestUnitsPageViewModel(IUnitService unit, ICommandService commandService)
        : base(PageId, commandService)
    {
        _unitService = unit;
        var un = unit.Units[VelocityBase.Id];
        _speed = new ReactiveProperty<double>(10);

        Speed = new HistoricalUnitProperty($"{PageId}.speed", _speed, un);
    }

    public HistoricalUnitProperty Speed { get; }

    protected override TestUnitsPageViewModel GetContext()
    {
        return this;
    }

    protected override void AfterLoadExtensions() { }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public override IExportInfo Source => SystemModule.Instance;
}
