namespace Asv.Avalonia;

public interface IUpdatableRttBoxViewModel<in TSelf, in T>
    where TSelf : IUpdatableRttBoxViewModel<TSelf, T>
{
    Action<TSelf, T> UpdateAction { get; }
}
