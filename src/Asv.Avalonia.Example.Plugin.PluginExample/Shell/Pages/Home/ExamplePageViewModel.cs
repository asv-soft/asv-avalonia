using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia.Example.Plugin.PluginExample;

public interface IExamplePageViewModel : IPage { }

public class ExamplePageViewModel : PageViewModel<IExamplePageViewModel>
{
    public const string PageId = "example";
    public const MaterialIconKind PageIcon = MaterialIconKind.Earth;

    public ExamplePageViewModel()
        : this(
            DesignTime.PageContext,
            DesignTime.CommandService,
            NullLoggerFactory.Instance,
            DesignTime.DialogService,
            DesignTime.ExtensionService
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public ExamplePageViewModel(
        IPageContext context,
        ICommandService cmd,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService extensionService
    )
        : base(PageId, context, cmd, loggerFactory, dialogService, extensionService) { }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions() { }
}
