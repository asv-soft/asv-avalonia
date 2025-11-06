using System.Diagnostics;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class TwoColumnRttBoxViewModel : RttBoxViewModel
{
    public TwoColumnRttBoxViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        Icon = MaterialIconKind.Ruler;
        Header = "Distance";
        Left.Header = "Left";
        Right.Header = "Right";
        Left.Units = "mm";
        Right.Units = "km/h";
        int index = 0;
        int maxIndex = Enum.GetValues<RttBoxStatus>().Length;
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
                if (Random.Shared.NextDouble() > 0.8)
                {
                    Left.Header = null;
                    Right.Header = null;
                }
                else
                {
                    Left.Header = "Left";
                    Right.Header = "Right";
                }
                if (Random.Shared.NextDouble() > 0.9)
                {
                    Left.ValueString = Asv.Avalonia.Units.NotAvailableString;
                    Right.ValueString = Asv.Avalonia.Units.NotAvailableString;
                }
                else
                {
                    Right.ValueString = (Random.Shared.Next(-6553500, 6553500) / 100.0).ToString(
                        "F2"
                    );
                    Left.ValueString = (Random.Shared.Next(-6553500, 6553500) / 100.0).ToString(
                        "F2"
                    );
                }

                Status = Enum.GetValues<RttBoxStatus>()[index++ % maxIndex];
                ProgressStatus = Enum.GetValues<RttBoxStatus>()[index++ % maxIndex];
                Updated();
            });
    }

    public TwoColumnRttBoxViewModel(
        NavigationId id,
        ILoggerFactory loggerFactory,
        TimeSpan? networkErrorTimeout = null
    )
        : base(id, loggerFactory, networkErrorTimeout) { }

    public KeyValueViewModel Left { get; } = new();
    public KeyValueViewModel Right { get; } = new();

    public string? StatusText
    {
        get;
        set => SetField(ref field, value);
    }
}

public class TwoColumnRttBoxViewModel<T> : TwoColumnRttBoxViewModel
{
    private readonly TimeSpan? _networkErrorTimeout;

    public TwoColumnRttBoxViewModel(
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

    public required Action<TwoColumnRttBoxViewModel<T>, T> UpdateAction { get; init; }

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
