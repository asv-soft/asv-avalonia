using System.Composition;
using Asv.Cfg;
using Avalonia.Controls;
using Avalonia.Input;
using Key = Avalonia.Remote.Protocol.Input.Key;

namespace Asv.Avalonia;

public partial class ShellView : UserControl
{
    public ShellView()
    {
        InitializeComponent();
    }
}
