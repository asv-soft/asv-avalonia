namespace Asv.Avalonia;

public interface IRttBoxViewModel : IHeadlinedViewModel
{
    bool? IsUpdated { get; }

    string? ShortHeader { get; }

    AsvColorKind Status { get; }

    double Progress { get; }

    AsvColorKind? ProgressStatus { get; }

    bool? IsNetworkError { get; }

    void Updated();
}
