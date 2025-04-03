using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(SetAltitudeDialogViewModel))]
public partial class SetAltitudeDialog : UserControl
{
    public SetAltitudeDialog()
    {
        InitializeComponent();
    }
}
