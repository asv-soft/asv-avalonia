using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using DotNext.Buffers;

namespace Asv.Avalonia.GeoMap;

public sealed class Tile : IDisposable
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
                    $"Tile: Created={_created}, Disposed={_disposed}, Alive={_created - _disposed}"
                );
            });
    }

    public static Tile Create(TileKey key, Stream stream, int size)
    {
        return new Tile(key, stream, size);
    }
    
    public static Tile Create(TileKey key, string path)
    {
        using var stream = File.OpenRead(path);
        return new Tile(key, stream, (int)stream.Length);
    }
    
    private int _refCount = 1;
    private Bitmap _value;
    private readonly byte[] _compressed;
    private readonly int _compressedSize;

    private Tile(TileKey key, Stream source, int size)
    {
        Key = key;
        _compressed = ArrayPool<byte>.Shared.Rent(size);
        _compressedSize = size;
        source.ReadExactly(_compressed, 0, size);

        _value = new Bitmap(new MemoryStream(_compressed, 0, _compressedSize));
        Size = (_value.PixelSize.Width * _value.PixelSize.Height * 4) + _compressedSize;
        Interlocked.Increment(ref _created);
    }

    public TileKey Key { get; }
    public long Size { get; }
    
    public void AddRef()
    {
        Interlocked.Increment(ref _refCount);
    }

    public void Save(string filePath)
    {
        File.WriteAllBytes(filePath, _compressed.AsSpan(0, _compressedSize));
    }

    public void Render(DrawingContext context, double x, double y)
    {
        context.DrawImage(_value, new Rect(x, y, _value.PixelSize.Width, _value.PixelSize.Height));
    }
    
    public void Dispose()
    {
        if (Interlocked.Decrement(ref _refCount) == 0)
        {
            Interlocked.Increment(ref _disposed);
            ArrayPool<byte>.Shared.Return(_compressed);
            _value.Dispose();
            _value = null!;
        }
    }

}
