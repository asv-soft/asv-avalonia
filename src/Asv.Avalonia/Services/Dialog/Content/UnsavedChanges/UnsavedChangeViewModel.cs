using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class UnsavedChangeViewModel : RoutableViewModel
{
    public const string ViewModelBaseId = "unsaved_change";

    public UnsavedChangeViewModel(
        UnsavedChangeMeta @base,
        ILoggerFactory loggerFactory,
        INavigationService navigationService
    )
        : base(NavigationId.GenerateByHash(ViewModelBaseId, @base.Page.Id), loggerFactory)
    {
        Base = @base;
        NavigatePage = new ReactiveCommand(_ =>
            navigationService.GoTo(Base.Page.GetPathToRoot())
        ).DisposeItWith(Disposable);
    }

    public UnsavedChangeMeta Base { get; }

    public ReactiveCommand NavigatePage { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
