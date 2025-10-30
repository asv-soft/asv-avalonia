using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class GeoPointPropertyViewModel : HeadlinedViewModel, IPropertyViewModel
{
    private readonly ReactiveProperty<GeoPoint> _modelValue;
    private readonly ReactiveProperty<double> _latitude;
    private readonly ReactiveProperty<double> _longitude;
    private readonly ReactiveProperty<double> _altitude;


    public GeoPointPropertyViewModel()
        : this(DesignTime.Id, new ReactiveProperty<GeoPoint>(), DesignTime.LoggerFactory, DesignTime.UnitService)
    {
        Header = "Position";
        Description = "Position description";
    }
    
    public GeoPointPropertyViewModel(NavigationId id, ReactiveProperty<GeoPoint> modelValue, ILoggerFactory loggerFactory, IUnitService unitService) 
        : base(id, loggerFactory)
    {
        _modelValue = modelValue;
        _latitude = new ReactiveProperty<double>().DisposeItWith(Disposable);
        _longitude = new ReactiveProperty<double>().DisposeItWith(Disposable);
        _altitude = new ReactiveProperty<double>().DisposeItWith(Disposable);
        _modelValue.DistinctUntilChanged()
            .Subscribe(x=>
            {
                _latitude.Value = x.Latitude;
                _longitude.Value = x.Longitude;
                _altitude.Value = x.Altitude;
            })
            .DisposeItWith(Disposable);
        
        _latitude.Subscribe(x => modelValue.Value = new GeoPoint(x, _longitude.Value, _altitude.Value)).DisposeItWith(Disposable);
        _longitude.Subscribe(x => modelValue.Value = new GeoPoint(_latitude.Value, x, _altitude.Value)).DisposeItWith(Disposable);
        _altitude.Subscribe(x => modelValue.Value = new GeoPoint(_latitude.Value, _longitude.Value, x)).DisposeItWith(Disposable);
        Latitude = new UnitPropertyViewModel(nameof(Latitude), _latitude, unitService[LatitudeBase.Id] ?? throw new NullReferenceException("Latitude Unit"), loggerFactory, "F7")
        {
            Parent = this,
            ShortName = "Lat",
            Icon = MaterialIconKind.Latitude,
        };
        Longitude = new UnitPropertyViewModel(nameof(Longitude), _longitude, unitService[LongitudeBase.Id] ?? throw new NullReferenceException("Longitude Unit"), loggerFactory, "F7")
        {
            Parent = this,
            ShortName = "Lon",
            Icon = MaterialIconKind.Longitude,
        };
        Altitude = new UnitPropertyViewModel(nameof(Altitude), _altitude, unitService[AltitudeBase.Id] ?? throw new NullReferenceException("Altitude Unit"), loggerFactory, "F2")
        {
            Parent = this,
            ShortName = "Alt",
            Icon = MaterialIconKind.Altimeter,
        };
    }
    public UnitPropertyViewModel Latitude { get; }
    public UnitPropertyViewModel Longitude { get; }
    public UnitPropertyViewModel Altitude { get;  }

    public void MoveTopLeft()
    {
        
    }
    
    public void MoveTopRight()
    {
        
    }
    public void MoveBottomLeft()
    {
        
    }
    public void MoveBottomRight()
    {
        
    }
    
    public void MoveUp()
    {
        
    }
    
    public void MoveDown()
    {
        
    }
    
    public void MoveLeft()
    {
        
    }
    
    public void MoveRight()
    {
        
    }
    
    
}