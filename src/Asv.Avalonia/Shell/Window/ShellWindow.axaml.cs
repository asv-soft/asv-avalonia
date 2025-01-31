using Avalonia;
using Avalonia.Controls;

namespace Asv.Avalonia;

public partial class ShellWindow : Window
{
    public ShellWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }
    
}
