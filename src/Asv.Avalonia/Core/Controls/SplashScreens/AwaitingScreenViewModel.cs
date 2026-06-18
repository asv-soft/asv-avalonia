using R3;

namespace Asv.Avalonia;

public class AwaitingScreenAction : IDisposable
{
    private readonly IAwaitingScreen _source;

    public AwaitingScreenAction(
        IAwaitingScreen source,
        string message,
        string header,
        double? progress = null,
        TimeSpan? delayForShowMessage = null
    )
    {
        _source = source;
        if (delayForShowMessage == null)
        {
            _source.Header = header;
            _source.Message = message;
            _source.IsActive = true;
            _source.Progress = progress;
        }
        else
        {
            _source.Header = header;
            _source.Message = message;
            _source.Progress = progress;
            _source.IsActive = false;
            Observable
                .Timer(delayForShowMessage.Value)
                .Subscribe(x =>
                {
                    _source.IsActive = true;
                });
        }
    }

    public void Dispose()
    {
        _source.IsActive = false;
        _source.Header = null;
        _source.Message = null;
        _source.Progress = null;
    }
}

public interface IAwaitingScreen
{
    bool IsActive { get; set; }
    string? Message { get; set; }
    string? Header { get; set; }
    double? Progress { get; set; }
}

public class AwaitingScreenViewModel(string id)
    : ViewModel(id),
        IAwaitingScreen
{
    public IDisposable? Show(string header, string message, TimeSpan? delayForShowMessage = null)
    {
        return null;
    }

    public bool IsActive
    {
        get;
        set => SetField(ref field, value);
    }

    public string? Message
    {
        get;
        set => SetField(ref field, value);
    }

    public string? Header
    {
        get;
        set => SetField(ref field, value);
    }

    public double? Progress
    {
        get;
        set => SetField(ref field, value);
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }
}
