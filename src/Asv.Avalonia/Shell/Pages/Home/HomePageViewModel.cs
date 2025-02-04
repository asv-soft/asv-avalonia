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
    }

    [ImportingConstructor]
    public HomePageViewModel(ICommandService cmd)
        : base(PageId, cmd) { }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }
}
