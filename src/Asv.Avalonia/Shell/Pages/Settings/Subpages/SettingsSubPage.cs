using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public abstract class SettingsSubPage(
    NavigationId id,
    ILayoutService layout,
    ILoggerFactory loggerFactory
) : TreeSubpage<ISettingsPage>(id, layout, loggerFactory), ISettingsSubPage
{
    public override ValueTask Init(ISettingsPage context) => ValueTask.CompletedTask;
}
