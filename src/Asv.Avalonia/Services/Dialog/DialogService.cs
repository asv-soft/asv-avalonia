using System.Collections.Immutable;

namespace Asv.Avalonia;

public sealed class DialogService : IDialogService
{
    private readonly ImmutableDictionary<string, ICustomDialog> _units;

    public DialogService(IEnumerable<ICustomDialog> items)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, ICustomDialog>();
        foreach (var item in items)
        {
            if (!builder.TryAdd(item.GetType().Name, item))
            {
                throw new InvalidOperationException($"Duplicate dialog type {item.GetType().Name}");
            }
        }

        _units = builder.ToImmutable();
    }

    public T GetDialogPrefab<T>()
        where T : class, ICustomDialog
    {
        _units.TryGetValue(typeof(T).Name, out var dialogRaw);
        if (dialogRaw is null)
        {
            throw new Exception($"Dialog with type \'{typeof(T).Name}\' not found");
        }

        var dialog =
            dialogRaw as T
            ?? throw new InvalidCastException($"Unable to cast dialog to {typeof(T).Name}");
        return dialog;
    }

    public bool TryGetDialogPrefab<T>(out T? dialog)
        where T : class, ICustomDialog
    {
        try
        {
            dialog = GetDialogPrefab<T>();
            return true;
        }
        catch (Exception)
        {
            dialog = null;
            return false;
        }
    }
}
