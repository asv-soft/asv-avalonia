using System.Collections.Generic;
using System.Composition;
using Asv.Cfg;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

public sealed class DocumentPageViewModelConfig : PageConfig { }

[ExportPage(PageId)]
public class DocumentPageViewModel
    : PageViewModel<DocumentPageViewModel, DocumentPageViewModelConfig>
{
    public const string PageId = "document";

    public DocumentPageViewModel()
        : this(DesignTime.CommandService, DesignTime.Configuration, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();

        Title = RS.DocumentPageViewModel_Title;
    }

    [ImportingConstructor]
    public DocumentPageViewModel(
        ICommandService cmd,
        IConfiguration cfg,
        ILoggerFactory loggerFactory
    )
        : base(PageId, cmd, cfg, loggerFactory)
    {
        Title = RS.DocumentPageViewModel_Title;
    }

    protected override DocumentPageViewModel GetContext()
    {
        return this;
    }

    protected override void AfterLoadExtensions() { }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public override IExportInfo Source => SystemModule.Instance;
}
