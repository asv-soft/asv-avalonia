namespace Asv.Avalonia;

internal class NullCustomDialog : ICustomDialog { }

public class NullDialogService : IDialogService
{
    public static IDialogService Instance { get; } = new NullDialogService();

    private NullDialogService() { }

#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
    public T? GetDialogPrefab<T>()
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
        where T : class, ICustomDialog
    {
        return null;
    }

    public bool TryGetDialogPrefab<T>(out T? dialog)
        where T : class, ICustomDialog
    {
        dialog = null;
        return true;
    }
}
