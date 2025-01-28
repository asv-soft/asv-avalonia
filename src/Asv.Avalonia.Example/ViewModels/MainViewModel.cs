using System;
using System.Collections.Generic;
using System.Composition;
using R3;

namespace Asv.Avalonia.Example.ViewModels;

public class Model : IDisposable
{
    public ReactiveProperty<double> Value1 { get; } = new(0);
    public ReactiveProperty<double> Value2 { get; } = new(0);

    public void Dispose()
    {
        Value1.Dispose();
        Value2.Dispose();
    }
}
