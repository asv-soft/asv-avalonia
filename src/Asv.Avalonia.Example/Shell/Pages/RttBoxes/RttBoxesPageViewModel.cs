using System;
using System.Collections.Generic;
using Asv.Common;
using Avalonia.Controls;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example;

[ExportControlExamples(PageId)]
public sealed class RttBoxesPageViewModel : ControlsGallerySubPage
{
    public const string PageId = "rtt-boxes";
    public const MaterialIconKind PageIcon = MaterialIconKind.PoundBox;

    public RttBoxesPageViewModel()
        : this(DesignTime.UnitService, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        Parent = DesignTime.Shell;
    }

    public RttBoxesPageViewModel(IUnitService unitService, ILoggerFactory loggerFactory)
        : base(PageId, loggerFactory)
    {
        GeoPointRttBoxViewModel = CreateGeoPointRttBoxViewModel(unitService, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        SplitDigitRttBoxViewModel = CreateSplitDigitRttBoxViewModel(unitService, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        KeyValueRttBoxViewModel = CreateKeyValueRttBoxViewModel(loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        SingleRttBoxViewModel = CreateSingleRttBoxViewModel(loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        TwoColumnRttBoxViewModel = CreateTwoColumnRttBoxViewModel(loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        RttBoxViewModel = CreateRttBoxViewModel(loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
    }

    public GeoPointRttBoxViewModel GeoPointRttBoxViewModel { get; }
    public SplitDigitRttBoxViewModel SplitDigitRttBoxViewModel { get; }
    public KeyValueRttBoxViewModel KeyValueRttBoxViewModel { get; }
    public SingleRttBoxViewModel SingleRttBoxViewModel { get; }
    public TwoColumnRttBoxViewModel TwoColumnRttBoxViewModel { get; }
    public RttBoxViewModel RttBoxViewModel { get; }

    public override IEnumerable<IRoutable> GetChildren()
    {
        yield return GeoPointRttBoxViewModel;
        yield return SplitDigitRttBoxViewModel;
        yield return KeyValueRttBoxViewModel;
        yield return SingleRttBoxViewModel;
        yield return TwoColumnRttBoxViewModel;
        yield return RttBoxViewModel;
        foreach (var child in base.GetChildren())
        {
            yield return child;
        }
    }

    private RttBoxViewModel CreateRttBoxViewModel(ILoggerFactory loggerFactory)
    {
        if (Design.IsDesignMode)
        {
            return new RttBoxViewModel
            {
                Icon = MaterialIconKind.Velocity,
                Header = "Velocity",
                IsNetworkError = true,
                ShortHeader = "vel",
            };
        }

        var viewModel = new RttBoxViewModel(nameof(RttBoxViewModel), loggerFactory);

        viewModel.Icon = MaterialIconKind.Velocity;
        viewModel.Header = "Velocity";
        viewModel.IsNetworkError = true;
        viewModel.ShortHeader = "vel";
        return viewModel;
    }

    private TwoColumnRttBoxViewModel CreateTwoColumnRttBoxViewModel(ILoggerFactory loggerFactory)
    {
        if (Design.IsDesignMode)
        {
            return new TwoColumnRttBoxViewModel();
        }

        var viewModel = new TwoColumnRttBoxViewModel(
            nameof(TwoColumnRttBoxViewModel),
            loggerFactory
        );

        viewModel.Icon = MaterialIconKind.Ruler;
        viewModel.Header = "Distance";
        viewModel.Left.Header = "Left";
        viewModel.Right.Header = "Right";
        viewModel.Left.UnitSymbol = "mm";
        viewModel.Right.UnitSymbol = "km/h";
        int index = 0;
        int maxIndex = Enum.GetValues<AsvColorKind>().Length;
        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2))
            .Subscribe(_ =>
            {
                if (Random.Shared.NextDouble() > 0.8)
                {
                    viewModel.IsNetworkError = true;
                    return;
                }

                viewModel.Progress = Random.Shared.NextDouble();
                if (Random.Shared.NextDouble() > 0.8)
                {
                    viewModel.Left.Header = null;
                    viewModel.Right.Header = null;
                }
                else
                {
                    viewModel.Left.Header = "Left";
                    viewModel.Right.Header = "Right";
                }
                if (Random.Shared.NextDouble() > 0.9)
                {
                    viewModel.Left.ValueString = Units.NotAvailableString;
                    viewModel.Right.ValueString = Units.NotAvailableString;
                }
                else
                {
                    viewModel.Right.ValueString = (
                        Random.Shared.Next(-6553500, 6553500) / 100.0
                    ).ToString("F2");
                    viewModel.Left.ValueString = (
                        Random.Shared.Next(-6553500, 6553500) / 100.0
                    ).ToString("F2");
                }

                viewModel.Status = Enum.GetValues<AsvColorKind>()[index++ % maxIndex];
                viewModel.ProgressStatus = Enum.GetValues<AsvColorKind>()[index++ % maxIndex];
                viewModel.Updated();
            })
            .DisposeItWith(Disposable);

        return viewModel;
    }

    private SingleRttBoxViewModel CreateSingleRttBoxViewModel(ILoggerFactory loggerFactory)
    {
        if (Design.IsDesignMode)
        {
            return new SingleRttBoxViewModel();
        }

        var viewModel = new SingleRttBoxViewModel(nameof(SingleRttBoxViewModel), loggerFactory);

        viewModel.Icon = MaterialIconKind.Ruler;
        viewModel.Header = "Distance";
        viewModel.UnitSymbol = "mm";

        int index = 0;
        int maxIndex = Enum.GetValues<AsvColorKind>().Length;
        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2))
            .Subscribe(_ =>
            {
                if (Random.Shared.NextDouble() > 0.8)
                {
                    viewModel.IsNetworkError = true;
                    return;
                }

                viewModel.Progress = Random.Shared.NextDouble();
                if (Random.Shared.NextDouble() > 0.9)
                {
                    viewModel.ValueString = Asv.Avalonia.Units.NotAvailableString;
                    viewModel.StatusText = "No data";
                }
                else
                {
                    viewModel.ValueString = (
                        Random.Shared.Next(-6553500, 6553500) / 100.0
                    ).ToString("F2");
                    viewModel.StatusText = null;
                }

                viewModel.Status = Enum.GetValues<AsvColorKind>()[index++ % maxIndex];
                viewModel.ProgressStatus = Enum.GetValues<AsvColorKind>()[index++ % maxIndex];
                viewModel.Updated();
            })
            .DisposeItWith(Disposable);

        return viewModel;
    }

    private KeyValueRttBoxViewModel CreateKeyValueRttBoxViewModel(ILoggerFactory loggerFactory)
    {
        if (Design.IsDesignMode)
        {
            return new KeyValueRttBoxViewModel();
        }

        var viewModel = new KeyValueRttBoxViewModel(nameof(KeyValueRttBoxViewModel), loggerFactory);

        viewModel.ShortHeader = "Short";
        viewModel.ShortValueString = "0.00";
        viewModel.ShortUnitSymbol = "ms";
        viewModel.Icon = MaterialIconKind.Radar;
        viewModel.Header = "Common RTT";

        KeyValueViewModel[] items =
        [
            new KeyValueViewModel(loggerFactory) { Header = "Power", UnitSymbol = "dBm" },
            new KeyValueViewModel(loggerFactory) { Header = "Rise time", UnitSymbol = "ms" },
            new KeyValueViewModel(loggerFactory) { Header = "Fall time", UnitSymbol = "ms" },
            new KeyValueViewModel(loggerFactory) { Header = "Status", ValueString = "Normal" },
            new KeyValueViewModel(loggerFactory) { Header = "Unknown" },
        ];

        viewModel.ItemsSource.AddRange(items);

        int index = 0;
        int maxIndex = Enum.GetValues<AsvColorKind>().Length;
        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2))
            .Subscribe(_ =>
            {
                for (var i = 0; i < viewModel.ItemsSource.Count; i++)
                {
                    var model = viewModel.ItemsSource[i];
                    model.ValueString = (Random.Shared.NextDouble() * 1000.0).ToString($"F{i}");
                }

                viewModel.Status = Enum.GetValues<AsvColorKind>()[index++ % maxIndex];
                viewModel.ProgressStatus = Enum.GetValues<AsvColorKind>()[index++ % maxIndex];
                viewModel.StatusText = viewModel.Status.ToString();
                viewModel.ShortValueString = (Random.Shared.NextDouble() * 1000.0).ToString("F2");
                viewModel.Updated();
            })
            .DisposeItWith(Disposable);

        return viewModel;
    }

    private SplitDigitRttBoxViewModel CreateSplitDigitRttBoxViewModel(
        IUnitService unitService,
        ILoggerFactory loggerFactory
    )
    {
        if (Design.IsDesignMode)
        {
            return new SplitDigitRttBoxViewModel();
        }

        var sub = new Subject<double>().DisposeItWith(Disposable);
        Observable<double> value = sub;

        var viewModel = new SplitDigitRttBoxViewModel(
            nameof(SplitDigitRttBoxViewModel),
            loggerFactory,
            unitService,
            DistanceUnit.Id,
            value,
            null
        );

        viewModel.Icon = MaterialIconKind.Ruler;
        viewModel.Header = "Distance";
        viewModel.FormatString = "## 000.000";
        int index = 0;
        int maxIndex = Enum.GetValues<AsvColorKind>().Length;
        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2))
            .Subscribe(_ =>
            {
                if (Random.Shared.NextDouble() > 0.8)
                {
                    viewModel.IsNetworkError = true;
                    return;
                }

                viewModel.Progress = Random.Shared.NextDouble();
                if (Random.Shared.NextDouble() > 0.9)
                {
                    sub.OnNext(double.NaN);
                }
                else
                {
                    sub.OnNext(Random.Shared.Next(-6553500, 6553500) / 100.0);
                }

                viewModel.Status = Enum.GetValues<AsvColorKind>()[index++ % maxIndex];
                viewModel.ProgressStatus = Enum.GetValues<AsvColorKind>()[index++ % maxIndex];
                viewModel.Updated();
            })
            .DisposeItWith(Disposable);

        return viewModel;
    }

    private GeoPointRttBoxViewModel CreateGeoPointRttBoxViewModel(
        IUnitService unitService,
        ILoggerFactory loggerFactory
    )
    {
        if (Design.IsDesignMode)
        {
            return new GeoPointRttBoxViewModel();
        }

        var viewModel = new GeoPointRttBoxViewModel(
            nameof(GeoPointRttBoxViewModel),
            loggerFactory,
            unitService,
            null
        );

        viewModel.GeoPointProperty.ModelValue.Value = new GeoPoint(55.75, 37.6173, 250.0);
        viewModel.Icon = MaterialIconKind.AddressMarker;
        viewModel.Header = "UAV position";
        viewModel.ShortHeader = "UAV";

        var index = 0;
        var maxIndex = Enum.GetValues<AsvColorKind>().Length;
        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2))
            .Subscribe(_ =>
            {
                if (Random.Shared.NextDouble() > 0.9)
                {
                    viewModel.IsNetworkError = true;
                    return;
                }

                viewModel.Status = Enum.GetValues<AsvColorKind>()[index++ % maxIndex];
                viewModel.ProgressStatus = Enum.GetValues<AsvColorKind>()[index++ % maxIndex];
                viewModel.Progress = Random.Shared.NextDouble();
                viewModel.StatusText = viewModel.Status.ToString();
            })
            .DisposeItWith(Disposable);

        return viewModel;
    }

    public override IExportInfo Source => SystemModule.Instance;
}
