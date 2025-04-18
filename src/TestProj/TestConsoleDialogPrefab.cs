using System.Reactive;
using Asv.Avalonia;

namespace TestProj;

public sealed class TestConsoleDialogPrefab : IDialogPrefab<EmptyDialogPayload, Unit>
{
    public Task<Unit> ShowDialogAsync(EmptyDialogPayload dialogDialogPayload)
    {
        Console.WriteLine("TestConsoleDialogPrefab.ShowDialogAsync");
        return Task.FromResult(Unit.Default);
    }
}
