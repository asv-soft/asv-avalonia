﻿using System.Windows.Input;
using R3;

namespace Asv.Avalonia;

public interface ICommandHistory
{
    string Id { get; }
    IDisposable Register(IViewModel context);
    void Unregister(IViewModel context);
    ReactiveCommand Undo { get; }
    ValueTask UndoAsync(CancellationToken cancel = default);
    ReactiveCommand Redo { get; }
    ValueTask RedoAsync(CancellationToken cancel = default);
    ValueTask Execute(ICommandBase command, IViewModel context, CancellationToken cancel = default);
}

public interface IQuickPick { }
