using ObservableCollections;

namespace Asv.Avalonia;

public interface ITreeSubpage : IRoutable, IExportable
{
    ObservableList<IMenuItem> Menu { get; }
}

public interface ITreeSubpage<in TContext> : ITreeSubpage
    where TContext : class, IPage
{
    ValueTask Init(TContext context);
}
