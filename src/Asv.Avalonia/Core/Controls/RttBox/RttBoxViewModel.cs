using Asv.Modeling;
using Material.Icons;

namespace Asv.Avalonia;

public class RttBoxViewModel : HeadlinedViewModel
{
    private readonly TimeProvider _timeProvider;
    private long _lastUpdate;

    public RttBoxViewModel()
        : base(DesignTime.Id.TypeId)
    {
        _timeProvider = TimeProvider.System;
        DesignTime.ThrowIfNotDesignMode();
    }

    public RttBoxViewModel(string id, TimeSpan? networkErrorTimeout = null)
        : base(id)
    {
        _timeProvider = TimeProvider.System;
        if (networkErrorTimeout is not null)
        {
            _timeProvider.CreateTimer(
                CheckNetworkTimeout,
                networkErrorTimeout,
                networkErrorTimeout.Value,
                networkErrorTimeout.Value
            );
        }
    }

    private void CheckNetworkTimeout(object? state)
    {
        if (state is null)
        {
            return;
        }

        if (state is not TimeSpan timeout)
        {
            throw new Exception($"{state} is not a {nameof(TimeSpan)}");
        }

        IsNetworkError = _timeProvider.GetElapsedTime(_lastUpdate) > timeout;
    }

    public bool? IsUpdated
    {
        get;
        private set => SetField(ref field, value);
    }

    public void Updated()
    {
        IsNetworkError = false;
        IsUpdated = false;
        IsUpdated = true;
        _lastUpdate = _timeProvider.GetTimestamp();
    }

    public string? ShortHeader
    {
        get;
        set => SetField(ref field, value);
    }

    public AsvColorKind Status
    {
        get;
        set => SetField(ref field, value);
    }

    public double Progress
    {
        get;
        set => SetField(ref field, value);
    }

    public AsvColorKind? ProgressStatus
    {
        get;
        set => SetField(ref field, value);
    }

    public bool? IsNetworkError
    {
        get;
        set => SetField(ref field, value);
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }
}
