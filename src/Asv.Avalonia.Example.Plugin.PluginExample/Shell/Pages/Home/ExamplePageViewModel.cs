using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia.Example.Plugin.PluginExample;

public interface IExamplePageViewModel : IPage { }

public class ExamplePageViewModel(
    ICommandService cmd,
    ILoggerFactory loggerFactory,
    IDialogService dialogService,
    IExtensionService ext
) : PageViewModel<IExamplePageViewModel>(PageId, cmd, loggerFactory, dialogService, ext)
{
    public const string PageId = "example";
    public const MaterialIconKind PageIcon = MaterialIconKind.Earth;

    public ExamplePageViewModel()
        : this(
            DesignTime.CommandService,
            NullLoggerFactory.Instance,
            DesignTime.DialogService,
            DesignTime.ExtensionService
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions() { }

}
