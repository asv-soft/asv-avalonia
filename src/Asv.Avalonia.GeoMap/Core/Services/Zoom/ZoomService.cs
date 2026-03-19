using Asv.Cfg;
using Asv.Common;
using R3;

namespace Asv.Avalonia.GeoMap;

public class ZoomServiceConfig
{
    public int MinZoom { get; set; } = IZoomService.MinZoomLevel;
    public int MaxZoom { get; set; } = IZoomService.MaxZoomLevel;
}

public class ZoomService : AsyncDisposableOnce, IZoomService
{
    private readonly Lock _syncCfg = new();

    public ZoomService(IConfiguration configProvider)
    {
        ArgumentNullException.ThrowIfNull(configProvider);

        var config = configProvider.Get<ZoomServiceConfig>();
        MinZoom = new SynchronizedReactiveProperty<int>(config.MinZoom);
        MaxZoom = new SynchronizedReactiveProperty<int>(config.MaxZoom);

        _sub1 = MinZoom
            .Skip(1)
            .Synchronize()
            .Subscribe(value =>
            {
                using (_syncCfg.EnterScope())
                {
                    config.MinZoom = value;
                    configProvider.Set(config);
                }
            });

        _sub2 = MaxZoom
            .Skip(1)
            .Synchronize()
            .Subscribe(value =>
            {
                using (_syncCfg.EnterScope())
                {
                    config.MaxZoom = value;
                    configProvider.Set(config);
                }
            });
    }

    public SynchronizedReactiveProperty<int> MinZoom { get; }
    public SynchronizedReactiveProperty<int> MaxZoom { get; }

    #region Dispose

    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            _sub2.Dispose();
            MinZoom.Dispose();
            MaxZoom.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
