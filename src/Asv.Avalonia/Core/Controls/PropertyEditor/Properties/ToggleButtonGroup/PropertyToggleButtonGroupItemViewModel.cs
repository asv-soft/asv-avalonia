using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Asv.Avalonia;

public sealed class PropertyToggleButtonGroupItemViewModel : INotifyPropertyChanged
{
    private bool _isSelected;

    internal PropertyToggleButtonGroupItemViewModel(IHeadlinedViewModel item)
    {
        ArgumentNullException.ThrowIfNull(item);
        Item = item;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IHeadlinedViewModel Item { get; }

    public bool IsSelected
    {
        get => _isSelected;
        internal set
        {
            if (_isSelected == value)
            {
                return;
            }

            _isSelected = value;
            OnPropertyChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
