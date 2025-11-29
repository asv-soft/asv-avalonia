using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia;

public class DialogItemUnsavedChangesViewModel(ILoggerFactory loggerFactory)
    : DialogViewModelBase(DialogId, loggerFactory)
{
    public const string DialogId = $"{BaseId}.item.unsaved_changes";

    public DialogItemUnsavedChangesViewModel()
        : this(NullLoggerFactory.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();

        Restrictions =
        [
            new Restriction(new SettingsPageViewModel(), "input 1 contains changes"),
            new Restriction(new SettingsPageViewModel(), "input 2 contains changes"),
            new Restriction(new SettingsPageViewModel(), "input 3 contains changes"),
        ];
    }

    public required IEnumerable<Restriction> Restrictions { get; init; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
