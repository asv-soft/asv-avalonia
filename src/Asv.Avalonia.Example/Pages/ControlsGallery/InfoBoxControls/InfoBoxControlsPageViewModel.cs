using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia.Example;

public sealed class InfoBoxControlsPageViewModelConfig
{
    public InfoBarSeverity Severity { get; set; } = InfoBarSeverity.Informational;
    public string InfoBoxTitle { get; set; } = string.Empty;
    public string InfoBoxMessage { get; set; } = string.Empty;
}

[ExportControlExamples(PageId)]
public class InfoBoxControlsPageViewModel : ControlsGallerySubPage
{
    public const string PageId = "info_box_controls";
    public const MaterialIconKind PageIcon = MaterialIconKind.InfoBox;

    private readonly ReactiveProperty<Enum> _severity;
    private readonly ReactiveProperty<string?> _infoBoxMessage;
    private readonly ReactiveProperty<string?> _infoBoxTitle;

    private InfoBoxControlsPageViewModelConfig _config;

    public InfoBoxControlsPageViewModel()
        : this(NullLayoutService.Instance, NullLoggerFactory.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public InfoBoxControlsPageViewModel(ILayoutService layoutService, ILoggerFactory loggerFactory)
        : base(PageId, layoutService, loggerFactory)
    {
        _severity = new ReactiveProperty<Enum>(InfoBarSeverity.Informational).DisposeItWith(
            Disposable
        );
        _infoBoxTitle = new ReactiveProperty<string?>(
            RS.InfoBoxControlsPageViewModel_Example_Title
        ).DisposeItWith(Disposable);
        _infoBoxMessage = new ReactiveProperty<string?>(
            RS.InfoBoxControlsPageViewModel_Example_Message
        ).DisposeItWith(Disposable);

        Severity = new HistoricalEnumProperty<InfoBarSeverity>(
            nameof(Severity),
            _severity,
            layoutService,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        InfoBoxTitle = new HistoricalStringProperty(
            nameof(InfoBoxTitle),
            _infoBoxTitle,
            layoutService,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        InfoBoxMessage = new HistoricalStringProperty(
            nameof(InfoBoxMessage),
            _infoBoxMessage,
            layoutService,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
    }

    public HistoricalEnumProperty<InfoBarSeverity> Severity { get; }
    public HistoricalStringProperty InfoBoxTitle { get; }
    public HistoricalStringProperty InfoBoxMessage { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return Severity;
        yield return InfoBoxTitle;
        yield return InfoBoxMessage;

        foreach (var child in base.GetRoutableChildren())
        {
            yield return child;
        }
    }

    protected override ValueTask HandleSaveLayout(CancellationToken cancel = default)
    {
        _config.Severity = Severity.ViewValue.Value;
        _config.InfoBoxTitle = InfoBoxTitle.ViewValue.Value ?? string.Empty;
        _config.InfoBoxMessage = InfoBoxMessage.ViewValue.Value ?? string.Empty;
        LayoutService.SetInMemory(this, _config);
        return base.HandleSaveLayout(cancel);
    }

    protected override ValueTask HandleLoadLayout(CancellationToken cancel = default)
    {
        _config = LayoutService.Get<InfoBoxControlsPageViewModelConfig>(this);
        Severity.ModelValue.Value = _config.Severity;
        if (!string.IsNullOrEmpty(_config.InfoBoxTitle))
        {
            InfoBoxTitle.ModelValue.Value = _config.InfoBoxTitle;
        }

        if (!string.IsNullOrEmpty(_config.InfoBoxMessage))
        {
            InfoBoxMessage.ModelValue.Value = _config.InfoBoxMessage;
        }

        return base.HandleLoadLayout(cancel);
    }

    public override IExportInfo Source => SystemModule.Instance;
}
