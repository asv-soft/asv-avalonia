using System.Composition;

namespace Asv.Avalonia;

[ExportPage(PageId)]
public class HomePageViewModel : PageViewModel<HomePageViewModel>
{
    public const string PageId = "home";

    public HomePageViewModel()
        : this(DesignTime.CommandService)
    {
        DesignTime.ThrowIfNotDesignMode();
        Title.OnNext(RS.HomePageVIewModel_Title);
    }

    [ImportingConstructor]
    public HomePageViewModel(ICommandService cmd)
        : base(PageId, cmd)
    {
        Title.OnNext(RS.HomePageVIewModel_Title);
    }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
