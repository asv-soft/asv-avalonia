using R3;

namespace Asv.Avalonia;

public interface ITreePage : IHeadlinedViewModel
{
    NavigationId ParentId { get; }
    TagViewModel? Status { get; }
    NavigationId NavigateTo { get; }
    ReactiveCommand NavigateCommand { get; }
}
