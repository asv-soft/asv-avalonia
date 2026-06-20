using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public enum TileDensity
{
    Regular,
    Inline = 2,
}

public interface ITileViewModel : IHeadlinedViewModel
{
    TileDensity Density { get; set; }
}

public class TileViewModel : HeadlinedViewModel, ITileViewModel
{
    private bool _isLayoutLoaded;

    public TileViewModel()
        : this(NavId.GenerateRandomAsString())
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public TileViewModel(string typeId)
        : base(typeId)
    {
        RegisterDensityLayout();
    }

    public TileDensity Density
    {
        get;
        set => SetField(ref field, value);
    }

    public string? ShortHeader
    {
        get;
        set => SetField(ref field, value);
    }

    public AsvColorKind StatusColor
    {
        get;
        set => SetField(ref field, value);
    }

    public MaterialIconKind? StatusIcon
    {
        get;
        set => SetField(ref field, value);
    }

    public AsvColorKind StatusIconColor
    {
        get;
        set => SetField(ref field, value);
    }

    public void MarkUpdated()
    {
        // Reset first so class-based Fadeout animations restart on repeated updates.
        StatusIconColor &= ~AsvColorKind.Fadeout;
        StatusIconColor |= AsvColorKind.Fadeout;
    }

    private void RegisterDensityLayout()
    {
        var densityLayout = Layout.Register<TileDensity>(
            nameof(Density),
            (value, _) =>
            {
                if (Enum.IsDefined(value))
                {
                    Density = value;
                }

                return ValueTask.CompletedTask;
            }
        );
        var densityLayoutSave = this.ObservePropertyChanged(x => x.Density)
            .Where(_ => _isLayoutLoaded)
            .SubscribeAwait(
                (_, cancel) => densityLayout.SaveAsync(Density, cancel),
                AwaitOperation.Drop
            );
        R3.Disposable.Combine(densityLayout, densityLayoutSave).DisposeItWith(Disposable);

        RootTracking.ExecuteWhenRootAttached(LoadLayoutWhenRootAttached).DisposeItWith(Disposable);
        return;

        async ValueTask LoadLayoutWhenRootAttached(IShell root, CancellationToken cancel)
        {
            _ = root;
            _isLayoutLoaded = false;
            try
            {
                await densityLayout.LoadAsync(cancel);
            }
            finally
            {
                if (!cancel.IsCancellationRequested && !IsDisposed)
                {
                    _isLayoutLoaded = true;
                }
            }
        }
    }
}
