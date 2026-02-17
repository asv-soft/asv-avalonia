using System;
using System.Collections.Generic;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia.Example;

[ExportControlExamples(PageId)]
public class InfoBoxControlsPageViewModel : ControlsGallerySubPage
{
    public const string PageId = "info_box_controls";
    public const MaterialIconKind PageIcon = MaterialIconKind.InfoBox;

    private readonly ReactiveProperty<Enum> _severity;
    private readonly ReactiveProperty<string?> _infoBoxMessage;
    private readonly ReactiveProperty<string?> _infoBoxTitle;

    public InfoBoxControlsPageViewModel()
        : this(NullLoggerFactory.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public InfoBoxControlsPageViewModel(ILoggerFactory loggerFactory)
        : base(PageId, loggerFactory)
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
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        InfoBoxTitle = new HistoricalStringProperty(
            nameof(InfoBoxTitle),
            _infoBoxTitle,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        InfoBoxMessage = new HistoricalStringProperty(
            nameof(InfoBoxMessage),
            _infoBoxMessage,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
    }

    public HistoricalEnumProperty<InfoBarSeverity> Severity { get; }
    public HistoricalStringProperty InfoBoxTitle { get; }
    public HistoricalStringProperty InfoBoxMessage { get; }

    public override IEnumerable<IRoutable> GetChildren()
    {
        yield return Severity;
        yield return InfoBoxTitle;
        yield return InfoBoxMessage;

        foreach (var child in base.GetChildren())
        {
            yield return child;
        }
    }

    public override IExportInfo Source => SystemModule.Instance;
}
