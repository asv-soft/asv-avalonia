using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Asv.Avalonia.Example.ViewModels;

public class MainViewModel : DisposableViewModel
{
    public MainViewModel()
        : base("shell")
    {
        History = new CommandHistory(Id);
        Property1 = new HistoryProperty(History, "property1");
        Property2 = new HistoryProperty(History, "property2");
    }

    public ICommandHistory History { get; }
    public HistoryProperty Property2 { get; }
    public HistoryProperty Property1 { get; }
}
