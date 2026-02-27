using System.Collections.Concurrent;
using Avalonia;
using Avalonia.Media.Imaging;

namespace Asv.Avalonia.GeoMap;

public sealed class ObjectPool<T>
    where T : class, new()
{
    private readonly ConcurrentBag<T> _bag = new();
    private int _count;
    private readonly int _maxSize;

    public ObjectPool(int maxSize = 1024)
    {
        _maxSize = maxSize;
    }

    public T Get()
    {
        if (_bag.TryTake(out var item))
        {
            Interlocked.Decrement(ref _count);
            return item;
        }
        return new T();
    }

    public void Return(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (Interlocked.Increment(ref _count) <= _maxSize)
        {
            _bag.Add(item);
        }
    }
}
