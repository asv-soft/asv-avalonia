using Asv.Cfg;
using R3;

namespace Asv.Avalonia;

public interface IConfigurable<TConfig>
    where TConfig : new()
{
    IConfiguration CfgService { get; init; }
    TConfig Config { get; init; }
    BindableReactiveProperty<bool> HasChanges { get; }

    ValueTask SaveChanges(CancellationToken cancellationToken);
}
