namespace Asv.Avalonia;

public interface IDialogService : IExportable
{
    public T GetDialogPrefab<T>()
        where T : class, ICustomDialog;

    public bool TryGetDialogPrefab<T>(out T? dialog)
        where T : class, ICustomDialog;
}
