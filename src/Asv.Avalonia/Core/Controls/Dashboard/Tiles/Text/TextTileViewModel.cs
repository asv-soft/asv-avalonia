using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public class TextTileViewModel : TileViewModel
{
    public TextTileViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        SetDefaultColors();
        SetDesignTimeData();
        StartDesignTimeAnimation();
    }

    public TextTileViewModel(string typeId)
        : base(typeId)
    {
        SetDefaultColors();
    }

    public string? Text
    {
        get;
        set => SetField(ref field, value);
    }

    public string? StatusText
    {
        get;
        set => SetField(ref field, value);
    }

    public AsvColorKind StatusTextColor
    {
        get;
        set => SetField(ref field, value);
    }

    public AsvColorKind TextColor
    {
        get;
        set => SetField(ref field, value);
    }

    public string? SuperScriptText
    {
        get;
        set => SetField(ref field, value);
    }

    public AsvColorKind SuperScriptTextColor
    {
        get;
        set => SetField(ref field, value);
    }

    public string? SubScriptText
    {
        get;
        set => SetField(ref field, value);
    }

    public AsvColorKind SubScriptTextColor
    {
        get;
        set => SetField(ref field, value);
    }

    public string? Units
    {
        get;
        set => SetField(ref field, value);
    }

    public double Progress
    {
        get;
        set => SetField(ref field, value);
    } = double.NaN;

    public AsvColorKind ProgressColor
    {
        get;
        set => SetField(ref field, value);
    }

    private void SetDefaultColors()
    {
        Icon = MaterialIconKind.Telecoil;
        IconColor = AsvColorKind.Error;
        StatusColor = AsvColorKind.Info3 | AsvColorKind.Blink;
        StatusIconColor = AsvColorKind.Unknown;
        StatusTextColor = AsvColorKind.Unknown;
        TextColor = AsvColorKind.Error;
    }

    private void SetDesignTimeData()
    {
        Density = TileDensity.Regular;
        Header = "GNSS 1 Status";
        ShortHeader = "GNSS1";
        Description =
            "- Fix: **RTK Fixed**\n"
            + "- Satellites: 18\n"
            + "- HDOP: 0.7\n"
            + "[color=Success;]Navigation solution is valid[/color]";

        Icon = MaterialIconKind.CrosshairsGps;
        IconColor = AsvColorKind.Info5;
        StatusColor = AsvColorKind.Success | AsvColorKind.Blink;
        StatusIcon = MaterialIconKind.CheckCircle;
        StatusIconColor = AsvColorKind.Success;

        Text = "47";
        SuperScriptText = "N";
        SuperScriptTextColor = AsvColorKind.Unknown;
        SubScriptText = ".397742";
        SubScriptTextColor = AsvColorKind.Unknown;
        Units = "deg";
        StatusText = "18 • HDOP 0.7";
        StatusTextColor = AsvColorKind.Success;
        Progress = 72;
        ProgressColor = AsvColorKind.Success;
    }

    private void StartDesignTimeAnimation()
    {
        var index = 0;
        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ => SetDesignTimeState(index++))
            .DisposeItWith(Disposable);
    }

    private void SetDesignTimeState(int index)
    {
        switch (index % 4)
        {
            case 0:
                Text = "47";
                SuperScriptText = "N";
                SubScriptText = ".397742";
                Units = "deg";
                StatusText = "18 • HDOP 0.7";
                StatusTextColor = AsvColorKind.Success;
                StatusIconColor = AsvColorKind.Success;
                Progress = 72;
                ProgressColor = AsvColorKind.Success;
                break;

            case 1:
                Text = "RTK";
                SuperScriptText = null;
                SubScriptText = "Fixed";
                Units = null;
                StatusText = "19 • HDOP 0.6";
                StatusTextColor = AsvColorKind.Success;
                StatusIconColor = AsvColorKind.Info5;
                Progress = 86;
                ProgressColor = AsvColorKind.Info5;
                break;

            case 2:
                Text = "3D";
                SuperScriptText = null;
                SubScriptText = "Float";
                Units = null;
                StatusText = "14 • HDOP 1.4";
                StatusTextColor = AsvColorKind.Warning;
                StatusIconColor = AsvColorKind.Warning;
                Progress = 48;
                ProgressColor = AsvColorKind.Warning;
                break;

            default:
                Text = "--";
                SuperScriptText = null;
                SubScriptText = null;
                Units = null;
                StatusText = "No fix";
                StatusTextColor = AsvColorKind.Error;
                StatusIconColor = AsvColorKind.Error;
                Progress = 12;
                ProgressColor = AsvColorKind.Error;
                break;
        }

        MarkUpdated();
    }
}
