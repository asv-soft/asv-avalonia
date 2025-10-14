using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public abstract class SettingsSubPage(
    NavigationId id,
    ILayoutService layoutService,
    ILoggerFactory loggerFactory
) : TreeSubpage<ISettingsPage>(id, layoutService, loggerFactory), ISettingsSubPage
{
    public override ValueTask Init(ISettingsPage context) => ValueTask.CompletedTask;
}
