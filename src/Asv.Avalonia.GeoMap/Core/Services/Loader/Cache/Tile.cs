using System.Buffers;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
#if DEBUG
using System.Diagnostics;
#endif

#if DEBUG
using R3;
#endif

namespace Asv.Avalonia.GeoMap;

public sealed class Tile : IDisposable
{
#if DEBUG
    private const string DebugCounterSwitchName = "Asv.Avalonia.GeoMap.Tile.DebugCounters";
    private static int _created;
    private static int _disposed;

    static Tile()
    {
        if (
            !AppContext.TryGetSwitch(DebugCounterSwitchName, out var isDebugCounterEnabled)
            || !isDebugCounterEnabled
        )
        {
            return;
        }

        Observable
            .Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(3000))
            .Subscribe(_ =>
            {
                Debug.WriteLine(
                    $"Tile: Created={_created}, Disposed={_disposed}, Alive={_created - _disposed}"
                );
            });
    }
#endif

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
    private byte[]? _compressed;
    private readonly int _compressedSize;

    private Tile(TileKey key, Stream source, int size)
    {
        Key = key;
        var compressed = ArrayPool<byte>.Shared.Rent(size);
        _compressed = compressed;
        _compressedSize = size;
        source.ReadExactly(compressed, 0, size);

        _value = new Bitmap(new MemoryStream(compressed, 0, _compressedSize));
        Size = _value.PixelSize.Width * _value.PixelSize.Height * 4;
#if DEBUG
        Interlocked.Increment(ref _created);
#endif
    }

    public TileKey Key { get; }
    public long Size { get; }
    public int CompressedSize => _compressedSize;

    public void AddRef()
    {
        Interlocked.Increment(ref _refCount);
    }

    public void Save(string filePath)
    {
        var compressed = _compressed;
        if (compressed == null)
        {
            throw new InvalidOperationException("Compressed tile data has already been released.");
        }

        File.WriteAllBytes(filePath, compressed.AsSpan(0, _compressedSize));
    }

    public void ReleaseCompressedBytes()
    {
        var compressed = Interlocked.Exchange(ref _compressed, null);
        if (compressed != null)
        {
            ArrayPool<byte>.Shared.Return(compressed);
        }
    }

    public void Render(DrawingContext context, double x, double y)
    {
        context.DrawImage(_value, new Rect(x, y, _value.PixelSize.Width, _value.PixelSize.Height));
    }

    public void Dispose()
    {
        if (Interlocked.Decrement(ref _refCount) == 0)
        {
#if DEBUG
            Interlocked.Increment(ref _disposed);
#endif
            ReleaseCompressedBytes();
            _value.Dispose();
            _value = null!;
        }
    }
}
