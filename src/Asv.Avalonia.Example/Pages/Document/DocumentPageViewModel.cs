using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;

namespace Asv.Avalonia.Example;

[ExportPage(PageId)]
public class DocumentPageViewModel : PageViewModel<DocumentPageViewModel>
{
    public const string PageId = "document";

    public DocumentPageViewModel()
        : this(DesignTime.CommandService)
    {
        DesignTime.ThrowIfNotDesignMode();
        
         // Title.OnNext(RS.DocumentPageViewModel_Title);
    }

    [ImportingConstructor]
    public DocumentPageViewModel(ICommandService cmd)
        : base(PageId, cmd)
    {
         // Title.OnNext(RS.DocumentPageViewModel_Title);
    }

    protected override DocumentPageViewModel GetContext()
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
