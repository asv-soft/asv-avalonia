using Asv.Modeling;

namespace Asv.Avalonia;

public interface IUndoStoreService
{
    IUndoHistoryStore CreateUndoHistoryStore(NavId ownerId);
}