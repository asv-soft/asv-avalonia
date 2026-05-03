using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia.Example.Plugin.PluginExample;

public interface IExamplePageViewModel : IPage { }

public class ExamplePageViewModel : PageViewModel<IExamplePageViewModel>, IExamplePageViewModel
{
    public const string PageId = "example";
    public const MaterialIconKind PageIcon = MaterialIconKind.Earth;

    public ExamplePageViewModel()
        : this(
            DesignTime.PageContext,
            
            DesignTime.LoggerFactory,
            DesignTime.DialogService,
            DesignTime.ExtensionService
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public ExamplePageViewModel(
        IPageContext context,
        
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService extensionService
    )
        : base(PageId, context,  loggerFactory, dialogService, extensionService)
    {
        Header = "Example page";
        Text1 = new BindableReactiveProperty<string>().DisposeItWith(Disposable);
        Undo.Register(nameof(Text1), Text1).DisposeItWith(Disposable);
        Text1.Value = "Hello world";
        Undo.EnableChangePublication();
    }

    public BindableReactiveProperty<string> Text1 { get; }
    
    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions() { }
}
