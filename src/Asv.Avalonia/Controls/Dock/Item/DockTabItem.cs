using Avalonia;
using Avalonia.Controls;

namespace Asv.Avalonia;

public partial class DockTabItem : TabItem
{
    public required string Id { get; init; }

    public new IPage? Content
    {
        get => base.Content as IPage;
        set => base.Content = value;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (
            change.Property == ContentProperty
            && change.NewValue is { } newValue
            && newValue is not IPage
        )
        {
            throw new InvalidOperationException(
                $"{nameof(DockTabItem)} supports only {nameof(IPage)} as Content."
            );
        }

        base.OnPropertyChanged(change);
    }
}
