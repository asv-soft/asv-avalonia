namespace Asv.Avalonia;

public interface IStatePersistor
{
    IPersistable Save();
    void Restore(IPersistable state);
}
