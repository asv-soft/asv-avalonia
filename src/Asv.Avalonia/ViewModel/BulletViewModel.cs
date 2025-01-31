using System.ComponentModel;
using System.Runtime.CompilerServices;
using Material.Icons;

namespace Asv.Avalonia;

public class BulletViewModel : INotifyPropertyChanged
{
    private string? _header;
    private MaterialIconKind _icon;

    public MaterialIconKind Icon
    {
        get => _icon;
        set => SetField(ref _icon, value);
    }

    public string? Header
    {
        get => _header;
        set => SetField(ref _header, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}