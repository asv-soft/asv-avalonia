using Asv.Modeling;

namespace Asv.Avalonia;

public interface IStatusItem : IViewModel
{
    int Order { get; }
}

public abstract class StatusItem(string typeId, NavArgs args)
    : ViewModel(typeId, args),
        IStatusItem
{
    public abstract int Order { get; }
}
