using System.Composition;
using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia.Example.Plugin.PluginExample;

public interface IExamplePageViewModel : IPage { }

public sealed class ExamplePageViewModelConfig : PageConfig { }

[ExportPage(PageId)]
[method: ImportingConstructor]
public class ExamplePageViewModel(
    ICommandService cmd,
    IConfiguration cfg,
    ILoggerFactory loggerFactory
)
    : PageViewModel<IExamplePageViewModel, ExamplePageViewModelConfig>(
        PageId,
        cmd,
        cfg,
        loggerFactory
    )
{
    public const string PageId = "example";
    public const MaterialIconKind PageIcon = MaterialIconKind.Earth;

    public ExamplePageViewModel()
        : this(DesignTime.CommandService, DesignTime.Configuration, NullLoggerFactory.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions() { }

    public override IExportInfo Source => SystemModule.Instance;
}
