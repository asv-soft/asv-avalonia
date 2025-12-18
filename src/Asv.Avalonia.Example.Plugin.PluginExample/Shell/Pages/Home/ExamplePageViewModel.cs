using System.Composition;
using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia.Example.Plugin.PluginExample;

public interface IExamplePageViewModel : IPage { }

[ExportPage(PageId)]
[method: ImportingConstructor]
public class ExamplePageViewModel(
    ICommandService cmd,
    ILoggerFactory loggerFactory,
    IDialogService dialogService
) : PageViewModel<IExamplePageViewModel>(PageId, cmd, loggerFactory, dialogService)
{
    public const string PageId = "example";
    public const MaterialIconKind PageIcon = MaterialIconKind.Earth;

    public ExamplePageViewModel()
        : this(DesignTime.CommandService, NullLoggerFactory.Instance, DesignTime.DialogService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions() { }

    public override IExportInfo Source => PluginExampleInfo.Instance;
}
