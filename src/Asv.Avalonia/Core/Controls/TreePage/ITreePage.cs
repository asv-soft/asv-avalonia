using Asv.Modeling;
using R3;

namespace Asv.Avalonia;

public interface ITreePage : IHeadlinedViewModel
{
    NavId ParentId { get; }
    TagViewModel? Status { get; }
    NavId NavigateTo { get; }
    ReactiveCommand NavigateCommand { get; }
}
