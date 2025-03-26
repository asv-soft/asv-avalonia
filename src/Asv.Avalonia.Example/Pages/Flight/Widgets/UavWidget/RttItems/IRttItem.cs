using System;

namespace Asv.Avalonia.Example;

public interface IRttItem : IDisposable
{
    public int Order { get; init; }
}