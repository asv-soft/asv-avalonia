using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public abstract class SettingsSubPage(NavigationId id, ILoggerFactory loggerFactory)
    : TreeSubpage<ISettingsPage>(id, loggerFactory),
        ISettingsSubPage
{
    public override ValueTask Init(ISettingsPage context) => ValueTask.CompletedTask;
}
