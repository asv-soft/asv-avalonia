using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia.Example;

public sealed class TestInfoBoxPageViewModelConfig : PageConfig { }

[ExportPage(PageId)]
public class TestInfoBoxPageViewModel
    : PageViewModel<TestInfoBoxPageViewModel, TestInfoBoxPageViewModelConfig>
{
    public const string PageId = "info-box";
    public const MaterialIconKind PageIcon = MaterialIconKind.TestTube;

    public TestInfoBoxPageViewModel()
        : this(DesignTime.CommandService, DesignTime.Configuration, NullLoggerFactory.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
        IsVisible.Value = true;
        Title = "Test infobox";
    }

    [ImportingConstructor]
    public TestInfoBoxPageViewModel(
        ICommandService cmd,
        IConfiguration cfg,
        ILoggerFactory logFactory
    )
        : base(PageId, cmd, cfg, logFactory)
    {
        Title = "Test infobox";

        IsVisible = new BindableReactiveProperty<bool>(false);
        ShowInfoBox = new ReactiveCommand(ShowInfoBoxImpl);
        HideInfoBox = new ReactiveCommand(HideInfoBoxImpl);
    }

    public ReactiveCommand ShowInfoBox { get; }
    public ReactiveCommand HideInfoBox { get; }
    public BindableReactiveProperty<bool> IsVisible { get; }

    private ValueTask ShowInfoBoxImpl(Unit unit, CancellationToken cancellationToken)
    {
        IsVisible.OnNext(true);

        return ValueTask.CompletedTask;
    }

    private ValueTask HideInfoBoxImpl(Unit unit, CancellationToken cancellationToken)
    {
        IsVisible.OnNext(false);

        return ValueTask.CompletedTask;
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions() { }

    public override IExportInfo Source => SystemModule.Instance;
}
