using ObservableCollections;

namespace Asv.Avalonia;

public interface ITreeSubpage : IViewModel
{
    MenuTree MenuView { get; }
    ObservableList<IMenuItem> Menu { get; }
}

public interface ITreeSubpage<in TContext> : ITreeSubpage
    where TContext : class, IPage
{
    ValueTask Init(TContext context);
}
