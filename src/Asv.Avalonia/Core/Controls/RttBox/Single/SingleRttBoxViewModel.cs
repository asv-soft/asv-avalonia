using System.Diagnostics;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class SingleRttBoxViewModel : RttBoxViewModel
{
    public SingleRttBoxViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        Icon = MaterialIconKind.Ruler;
        Header = "Distance";
        UnitSymbol = "mm";
        int index = 0;
        int maxIndex = Enum.GetValues<AsvColorKind>().Length;
        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2))
            .Subscribe(_ =>
            {
                if (Random.Shared.NextDouble() > 0.8)
                {
                    IsNetworkError = true;
                    return;
                }

                Progress = Random.Shared.NextDouble();
                if (Random.Shared.NextDouble() > 0.9)
                {
                    ValueString = Asv.Avalonia.Units.NotAvailableString;
                    StatusText = "No data";
                }
                else
                {
                    ValueString = (Random.Shared.Next(-6553500, 6553500) / 100.0).ToString("F2");
                    StatusText = null;
                }

                Status = Enum.GetValues<AsvColorKind>()[index++ % maxIndex];
                ProgressStatus = Enum.GetValues<AsvColorKind>()[index++ % maxIndex];
                Updated();
            })
            .DisposeItWith(Disposable);
    }

    public SingleRttBoxViewModel(
        NavigationId id,
        ILoggerFactory loggerFactory,
        TimeSpan? networkErrorTimeout = null
    )
        : base(id, loggerFactory, networkErrorTimeout) { }

    public string? UnitSymbol
    {
        get;
        set => SetField(ref field, value);
    }

    public string? ValueString
    {
        get;
        set => SetField(ref field, value);
    }

    public string? StatusText
    {
        get;
        set => SetField(ref field, value);
    }
}

public class SingleRttBoxViewModel<T> : SingleRttBoxViewModel
{
    private readonly TimeSpan? _networkErrorTimeout;

    public SingleRttBoxViewModel(
        NavigationId id,
        ILoggerFactory loggerFactory,
        Observable<T> valueStream,
        TimeSpan? networkErrorTimeout
    )
        : base(id, loggerFactory, networkErrorTimeout)
    {
        _networkErrorTimeout = networkErrorTimeout;
        valueStream
            .ThrottleLastFrame(1)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(OnValueChanged)
            .DisposeItWith(Disposable);
    }

    public required Action<SingleRttBoxViewModel<T>, T> UpdateAction { get; init; }

    private void OnValueChanged(T value)
    {
        Debug.Assert(UpdateAction != null, "UpdateAction must be set");
        UpdateAction(this, value);
        if (_networkErrorTimeout != null)
        {
            Updated();
        }
    }
}
