using Asv.Modeling;

namespace Asv.Avalonia;

public interface ITreePageMenuItem : IHeadlinedViewModel
{
    NavId ParentId { get; }
    TagViewModel? Status { get; }
    NavId NavigateTo { get; }
}
