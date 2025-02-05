using System.Composition;

namespace Asv.Avalonia;

[ExportPage(PageId)]
public class DocumentViewModel : PageViewModel<DocumentViewModel>
{
    public const string PageId = "document";

    public DocumentViewModel()
        : this(DesignTime.CommandService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public DocumentViewModel(ICommandService cmd)
        : base(PageId, cmd) { }

    protected override void AfterLoadExtensions()
    {
    }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}