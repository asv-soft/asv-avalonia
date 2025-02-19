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

        Title.OnNext(RS.HomePageViewModel_Title);
    }

    [ImportingConstructor]
    public HomePageViewModel(ICommandService cmd)
        : base(PageId, cmd)
    {
        Title.OnNext(RS.HomePageViewModel_Title);
    }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    protected override HomePageViewModel GetContext()
    {
        return this;
    }

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        if (e is PageCloseAttemptEvent attempt)
        {
            var restriction = new PageCloseRestriction(this, PageId);
            attempt.AddRestriction(restriction);
        }

        return base.InternalCatchEvent(e);
    }

    public override IExportInfo Source => SystemModule.Instance;
}
