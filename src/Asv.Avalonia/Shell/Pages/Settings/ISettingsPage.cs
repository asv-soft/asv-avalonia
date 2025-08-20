using Asv.Cfg;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia;

public interface ISettingsPage : IPage
{
    ObservableList<ITreePage> Nodes { get; }
}

public interface ISettingsSubPage : ITreeSubpage<ISettingsPage> { }

public abstract class SettingsSubPage<TConfig>(
    NavigationId id,
    IConfiguration cfg,
    ILoggerFactory loggerFactory
) : TreeSubpage<ISettingsPage, TConfig>(id, cfg, loggerFactory), ISettingsSubPage
    where TConfig : TreeSubpageConfig, new()
{
    public override ValueTask Init(ISettingsPage context) => ValueTask.CompletedTask;

    public override IEnumerable<IRoutable> GetRoutableChildren() => Menu;
}
