using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Example;

public partial class Window1 : Window
{
    public Window1()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }
}