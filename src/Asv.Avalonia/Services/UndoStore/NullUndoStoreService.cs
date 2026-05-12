using Asv.Modeling;

namespace Asv.Avalonia;

public class NullUndoStoreService : IUndoStoreService
{
    public IUndoHistoryStore CreateUndoHistoryStore(NavId ownerId)
    {
        return NullUndoHistoryStore.Instance;
    }
}
