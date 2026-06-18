using Asv.Modeling;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public enum TileDensity
{
    Regular,
    Compact,
    Inline,
}

public interface ITileViewModel : IHeadlinedViewModel
{
    TileDensity Density { get; set; }
}

public class TileViewModel : HeadlinedViewModel, ITileViewModel
{
    public TileViewModel() 
        : base(NavId.GenerateRandomAsString())
    {
        DesignTime.ThrowIfNotDesignMode();
    }
    
    public TileViewModel(string typeId) 
        : base(typeId)
    {
        
    }

    public TileDensity Density
    {
        get;
        set => SetField(ref field, value);
    }
    
    public bool UpdatedFlag
    {
        get;
        private set => SetField(ref field, value);
    }

    public string? ShortHeader
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsBusy
    {
        get;
        protected set => SetField(ref field, value);
    }

    public MaterialIconKind ErrorIcon
    {
        get;
        set => SetField(ref field, value);
    } = MaterialIconKind.CloseNetwork;

    public string? ErrorMessage
    {
        get;
        set => SetField(ref field, value);
    }

    protected void MarkUpdated()
    {
        UpdatedFlag = false;
        UpdatedFlag = true;
    }
}