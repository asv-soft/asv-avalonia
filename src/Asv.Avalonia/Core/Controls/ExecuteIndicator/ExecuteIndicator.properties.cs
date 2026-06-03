using Avalonia;

namespace Asv.Avalonia;

public partial class ExecuteIndicator
{
    public static readonly StyledProperty<bool> IsExecutingProperty = AvaloniaProperty.Register<
        ExecuteIndicator,
        bool
    >(nameof(IsExecuting));

    public bool IsExecuting
    {
        get => GetValue(IsExecutingProperty);
        set => SetValue(IsExecutingProperty, value);
    }
}
