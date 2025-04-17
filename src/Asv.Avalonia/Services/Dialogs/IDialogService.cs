namespace Asv.Avalonia;

public interface IDialogService
{
    IReadOnlyDictionary<string, ICustomDialog> Dialogs { get; }

    public T GetDialogPrefab<T>()
        where T : class, ICustomDialog;
}
