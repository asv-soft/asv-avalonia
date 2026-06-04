using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace Asv.Avalonia;

[PseudoClasses(":executing")]
public partial class ExecuteIndicator : TemplatedControl
{
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsExecutingProperty)
        {
            PseudoClasses.Set(":executing", IsExecuting);
        }
    }
}
