using System.Composition;
using Asv.Avalonia.Example.Api;
using Asv.Cfg;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia.Example.Plugin.PluginExample;

public interface IExamplePageViewModel : IPage { }

[ExportPage(PageId)]
public class ExamplePageViewModel : PageViewModel<IExamplePageViewModel>
{
    public const string PageId = "example";
    public const MaterialIconKind PageIcon = MaterialIconKind.Earth;

    public ExamplePageViewModel()
        : this(DesignTime.CommandService, NullLoggerFactory.Instance, DesignTime.DialogService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public ExamplePageViewModel(
        ICommandService cmd,
        ILoggerFactory loggerFactory,
        IDialogService dialogService
    )
        : base(PageId, cmd, loggerFactory, dialogService) { }

    public override IEnumerable<IRoutable> GetChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions() { }

    public override IExportInfo Source => PluginExampleInfo.Instance;
}
