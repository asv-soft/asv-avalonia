using Asv.Modeling;
using Material.Icons;

namespace Asv.Avalonia;

public class TextTileViewModel : TileViewModel
{
    public TextTileViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        SetDefaultColors();
        SetDesignTimeData();
    }

    public TextTileViewModel(string typeId)
        : base(typeId)
    {
        SetDefaultColors();
    }

    public AsvColorKind HeaderColor
    {
        get;
        set => SetField(ref field, value);
    }

    public AsvColorKind StatusColor
    {
        get;
        set => SetField(ref field, value);
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

    public AsvColorKind UnitsColor
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

        Text = "47";
        SuperScriptText = "N";
        SuperScriptTextColor = AsvColorKind.Unknown;
        SubScriptText = ".397742";
        SubScriptTextColor = AsvColorKind.Unknown;
        Units = "deg";
        UnitsColor = AsvColorKind.Unknown;

        StatusText = "18 • HDOP 0.7";
        StatusTextColor = AsvColorKind.Success;
        Progress = 72;
        ProgressColor = AsvColorKind.Success;
    }
}
