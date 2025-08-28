using Asv.Cfg;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public abstract class SettingsSubPage<TConfig>(
    NavigationId id,
    IConfiguration cfg,
    ILoggerFactory loggerFactory
) : TreeSubpage<ISettingsPage>(id, loggerFactory), ISettingsSubPage
    where TConfig : TreeSubpageConfig, new()
{
    public override ValueTask Init(ISettingsPage context) => ValueTask.CompletedTask;

    public override IEnumerable<IRoutable> GetRoutableChildren() => Menu;
}
