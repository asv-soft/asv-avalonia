using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia;

public class DialogItemUnsavedChangesViewModel : DialogViewModelBase
{
    public const string DialogId = $"{BaseId}.item.unsaved_changes";

    public DialogItemUnsavedChangesViewModel()
        : this(
            NullLoggerFactory.Instance,
            NullNavigationService.Instance,
            [
                new UnsavedChangeMeta
                {
                    Page = new HomePageViewModel(),
                    Restrictions = ["Some restriction"],
                },
                new UnsavedChangeMeta
                {
                    Page = new SettingsPageViewModel(),
                    Restrictions = ["Restriction 1", "Restriction 2"],
                },
            ]
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public DialogItemUnsavedChangesViewModel(
        ILoggerFactory loggerFactory,
        INavigationService navigationService,
        List<UnsavedChangeMeta> changes
    )
        : base(DialogId, loggerFactory)
    {
        Changes = changes.ConvertAll(c =>
            new UnsavedChangeViewModel(c, loggerFactory, navigationService).SetRoutableParent(this)
        );
    }

    public List<UnsavedChangeViewModel> Changes { get; init; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        foreach (var item in Changes)
        {
            yield return item;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var item in Changes)
            {
                item.Dispose();
            }
        }

        base.Dispose(disposing);
    }
}
