using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Media.Imaging;

namespace Asv.Avalonia.GeoMap;

public class Tile : IDisposable
{
    private static int _created;
    private static int _disposed;

    static Tile()
    {
        Observable
            .Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(3000))
            .Subscribe(_ =>
            {
                Debug.WriteLine(
                    $"Created={_created}, Disposed={_disposed}, Alive={_created - _disposed}"
                );
            });
    }
    
    private int _refCount = 1;
    private readonly int _id;
    private byte[] _data;

    public Tile(TileKey key, Stream stream)
    {
        Key = key;
        using var bitmap = WriteableBitmap.Decode(stream);
        PixelSize = bitmap.PixelSize;
        Dpi = bitmap.Dpi;
        using var buffer = bitmap.Lock();
        DataSize = buffer.Size.Width * buffer.Size.Height * 4;
        _data = ArrayPool<byte>.Shared.Rent(DataSize);

        _id = Interlocked.Increment(ref _created);
        Debug.WriteIf(_id == 1, $"=>>{_id:000}:+ ctor {_refCount}\n");

        // Копируем данные
        unsafe
        {
            byte* src = (byte*)buffer.Address;

            fixed (byte* dest = _data)
            {
                if (buffer.RowBytes == buffer.Size.Width * 4)
                {
                    // Самый частый и быстрый случай — stride совпадает с шириной × 4
                    Buffer.MemoryCopy(src, dest, DataSize, DataSize);
                }
                else
                {
                    // Редкий случай: есть padding в строках (почти никогда не бывает в Bgra8888,
                    // но для надёжности обрабатываем построчно)
                    int srcStride = buffer.RowBytes;
                    int destStride = buffer.Size.Width * 4;
                    int copyBytesPerRow = destStride;

                    byte* pDest = dest;

                    for (int y = 0; y < buffer.Size.Height; y++)
                    {
                        Buffer.MemoryCopy(src, pDest, copyBytesPerRow, copyBytesPerRow);
                        src += srcStride;
                        pDest += destStride;
                    }
                }
            }
        }
    }

    public void Write(string file)
    {
        if (_data == null)
        {
            throw new ObjectDisposedException(nameof(Tile));
        }

        using var stream = File.Create(file);
        using var bitmap = new WriteableBitmap(PixelSize, Dpi);
        using var buffer = bitmap.Lock();
        unsafe
        {
            byte* dest = (byte*)buffer.Address;
            fixed (byte* src = _data)
            {
                Buffer.MemoryCopy(src, dest, DataSize, DataSize);
            }
        }
        bitmap.Save(stream);
    }

    public void Write(WriteableBitmap bitmap)
    {
        if (_data == null)
        {
            throw new ObjectDisposedException(nameof(Tile));
        }

        using var buffer = bitmap.Lock();
        unsafe
        {
            var src = (byte*)buffer.Address;
            fixed (byte* dest = _data)
            {
                Buffer.MemoryCopy(dest, src, DataSize, DataSize);
            }
        }
    }

    public void AddRef()
    {
        Interlocked.Increment(ref _refCount);
        Debug.WriteIf(_id == 1, $"=>>{_id:000}:+ add {_refCount}\n");
    }

    public void Dispose()
    {
        Debug.WriteIf(_id == 1, $"=>>{_id:000}:- dispose {_refCount - 1}\n");
        if (Interlocked.Decrement(ref _refCount) == 0)
        {
            Debug.WriteIf(_id == 1, $"=>>{_id:000}:- FINISH!!!!!!!!!!!!!!! {_refCount}\n");
            Interlocked.Increment(ref _disposed);
            ArrayPool<byte>.Shared.Return(_data);
            _data = null!;
        }
    }
    public PixelSize PixelSize { get; }
    public Vector Dpi { get; }
    public TileKey Key { get; }
    public int DataSize { get; }
    
}

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
