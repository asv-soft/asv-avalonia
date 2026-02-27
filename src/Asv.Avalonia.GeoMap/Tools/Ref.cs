using System.Diagnostics;
using R3;

namespace Asv.Avalonia.GeoMap;

public sealed class Ref<T> : IDisposable
    where T : class, IDisposable
{
    public static int Created;
    public static int Disposed;

    static Ref()
    {
        Observable
            .Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(3000))
            .Subscribe(_ =>
            {
                Debug.WriteLine(
                    $"Ref<{typeof(T).Name}>: Created={Created}, Disposed={Disposed}, Alive={Created - Disposed}"
                );
            });
    }

    public Ref(T value)
    {
        _value = value;
        _id = Interlocked.Increment(ref Created);
        Debug.WriteIf(_id == 1, $"=>>{_id:000}:+ ctor {_refCount}\n");
    }

    private T? _value;
    private int _refCount = 1;
    private readonly int _id;

    public T Value
    {
        get
        {
            if (_value == null)
            {
                throw new ObjectDisposedException(nameof(Ref<T>));
            }

            return _value;
        }
    }

    public Ref<T> AddRef()
    {
        Interlocked.Increment(ref _refCount);
        Debug.WriteIf(_id == 1, $"=>>{_id:000}:+ add {_refCount}\n");
        return this;
    }

    public void Dispose()
    {
        Debug.WriteIf(_id == 1, $"=>>{_id:000}:- dispose {_refCount - 1}\n");
        if (Interlocked.Decrement(ref _refCount) == 0)
        {
            Debug.WriteIf(_id == 1, $"=>>{_id:000}:- FINISH!!!!!!!!!!!!!!! {_refCount}\n");
            Interlocked.Increment(ref Disposed);
            _value?.Dispose();
            _value = null;
        }
    }
}
