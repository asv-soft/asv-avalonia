using Asv.Modeling;

namespace Asv.Avalonia;

public interface ITreePage : IHeadlinedViewModel
{
    NavId ParentId { get; }
    TagViewModel? Status { get; }
    NavId NavigateTo { get; }
}
