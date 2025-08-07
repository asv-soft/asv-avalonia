using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia.Example;

[ExportControlExamples(PageId)]
public class InfoBoxControlsPageViewModel
    : TreeSubpage<ControlsGalleryPageViewModel>,
        IControlsGallerySubPage
{
    public const string PageId = "info_box_controls";
    public const MaterialIconKind PageIcon = MaterialIconKind.TestTube;

    private readonly ReactiveProperty<string?> _infoBoxTitle;
    private readonly ReactiveProperty<string?> _infoBoxMessage;

    public InfoBoxControlsPageViewModel()
        : this(NullLoggerFactory.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public InfoBoxControlsPageViewModel(ILoggerFactory loggerFactory)
        : base(PageId, loggerFactory)
    {
        Severity = new BindableReactiveProperty<InfoBarSeverity>(
            InfoBarSeverity.Informational
        ).DisposeItWith(Disposable);

        _infoBoxTitle = new ReactiveProperty<string?>(
            RS.InfoBoxPageViewModel_Example_Title
        ).DisposeItWith(Disposable);
        _infoBoxMessage = new ReactiveProperty<string?>(
            RS.InfoBoxPageViewModel_Example_Message
        ).DisposeItWith(Disposable);

        InfoBoxTitle = new HistoricalStringProperty(
            nameof(InfoBoxTitle),
            _infoBoxTitle,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        InfoBoxMessage = new HistoricalStringProperty(
            nameof(InfoBoxTitle),
            _infoBoxMessage,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
    }

    public HistoricalStringProperty InfoBoxTitle { get; }
    public HistoricalStringProperty InfoBoxMessage { get; }
    public BindableReactiveProperty<InfoBarSeverity> Severity { get; }
    public InfoBarSeverity[] Severities { get; } = Enum.GetValues<InfoBarSeverity>();

    public override ValueTask Init(ControlsGalleryPageViewModel context) => ValueTask.CompletedTask;

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return InfoBoxTitle;
        yield return InfoBoxMessage;
    }

    public override IExportInfo Source => SystemModule.Instance;
}
