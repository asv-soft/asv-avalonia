using System.Composition;

namespace Asv.Avalonia;

[ExportPage(PageId)]
public class DocumentViewModel: PageViewModel<DocumentViewModel>
{
    public const string PageId = "document";

    [ImportingConstructor]
    public DocumentViewModel(string id, ICommandService cmd)
        : base(id, cmd)
    {
    }

    public override ValueTask<IRoutable> NavigateTo(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    protected override void AfterLoadExtensions()
    {
    }
}