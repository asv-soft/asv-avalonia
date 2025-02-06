using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Caching.Memory;

namespace Asv.Avalonia.Example;

public static class BingMapsHelper
{
    public static string LatLongToQuadKey(double latitude, double longitude, int zoom)
    {
        int tileX,
            tileY;
        LatLongToTileXY(latitude, longitude, zoom, out tileX, out tileY);
        return TileXYToQuadKey(tileX, tileY, zoom);
    }

    public static void LatLongToTileXY(
        double latitude,
        double longitude,
        int zoom,
        out int tileX,
        out int tileY
    )
    {
        double sinLatitude = Math.Sin(latitude * Math.PI / 180);
        double pixelX = ((longitude + 180.0) / 360.0) * 256 * (1 << zoom);
        double pixelY =
            (0.5 - (Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI)))
            * 256
            * (1 << zoom);

        tileX = (int)(pixelX / 256);
        tileY = (int)(pixelY / 256);
    }

    public static string TileXYToQuadKey(int tileX, int tileY, int zoom)
    {
        char[] quadKey = new char[zoom];
        for (int i = zoom; i > 0; i--)
        {
            char digit = '0';
            int mask = 1 << (i - 1);
            if ((tileX & mask) != 0)
            {
                digit++;
            }

            if ((tileY & mask) != 0)
            {
                digit += (char)2;
            }

            quadKey[zoom - i] = digit;
        }

        return new string(quadKey);
    }
}

public class BingTileLoader
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private readonly string _apiKey;

    public BingTileLoader(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<Bitmap?> LoadTileAsync(int tileX, int tileY, int zoom)
    {
        string quadKey = BingMapsHelper.TileXYToQuadKey(tileX, tileY, zoom);
        string url =
            $"https://t0.ssl.ak.dynamic.tiles.virtualearth.net/comp/CompositionHandler/{quadKey}?mkt=en-US&it=A,G,L&key={_apiKey}";

        try
        {
            var imageBytes = await _httpClient.GetByteArrayAsync(url);
            return new Bitmap(new MemoryStream(imageBytes));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки тайла: {ex.Message}");
            return null;
        }
    }
}

public class HybridTileCache : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly MemoryCache _memoryCache;
    private readonly MemoryPool<byte> _memoryPool;
    private readonly string _cachePath = "map"; // Папка для хранения тайлов
    private readonly ConcurrentDictionary<string, Task<Bitmap?>> _loadingTiles; // Избегаем дублирующихся запросов

    public HybridTileCache(string apiKey)
    {
        _httpClient = new HttpClient();
        _apiKey = apiKey;
        _memoryCache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 500 });
        _memoryPool = MemoryPool<byte>.Shared;
        _loadingTiles = new ConcurrentDictionary<string, Task<Bitmap?>>();

        // Создаем папку для кэша
        Directory.CreateDirectory(_cachePath);
    }

    public async Task<Bitmap?> GetTileAsync(int tileX, int tileY, int zoom)
    {
        var cacheKey = $"{zoom}/{tileX}/{tileY}";
        var filePath = Path.Combine(_cachePath, $"{zoom}_{tileX}_{tileY}.png");

        // 1. Проверяем в памяти
        if (_memoryCache.TryGetValue(cacheKey, out IMemoryOwner<byte>? cachedTileMemory))
        {
            if (cachedTileMemory != null)
            {
                return CreateBitmapFromMemory(cachedTileMemory.Memory);
            }
        }

        // 2. Проверяем на диске
        if (File.Exists(filePath))
        {
            var diskTile = new Bitmap(filePath);
            _memoryCache.Set(cacheKey, diskTile, new MemoryCacheEntryOptions { Size = 1 });
            return diskTile;
        }

        // 3. Проверяем, не загружается ли уже этот тайл
        if (_loadingTiles.TryGetValue(cacheKey, out var existingTask))
        {
            return await existingTask;
        }

        // 4. Загружаем тайл
        var newTask = LoadTileAsync(tileX, tileY, zoom, cacheKey, filePath);
        _loadingTiles.TryAdd(cacheKey, newTask);
        var bitmap = await newTask;
        _loadingTiles.TryRemove(cacheKey, out _);
        return bitmap;
    }

    private async Task<Bitmap?> LoadTileAsync(
        int tileX,
        int tileY,
        int zoom,
        string cacheKey,
        string filePath
    )
    {
        string quadKey = BingMapsHelper.TileXYToQuadKey(tileX, tileY, zoom);
        string url =
            $"https://t0.ssl.ak.dynamic.tiles.virtualearth.net/comp/CompositionHandler/{quadKey}?mkt=en-US&it=A,G,L&key={_apiKey}";

        try
        {
            using var response = await _httpClient.GetAsync(
                url,
                HttpCompletionOption.ResponseHeadersRead
            );
            response.EnsureSuccessStatusCode();

            var contentLength = response.Content.Headers.ContentLength ?? 0;
            if (contentLength == 0)
            {
                return null;
            }

            using var memoryOwner = _memoryPool.Rent((int)contentLength);

            using var stream = await response.Content.ReadAsStreamAsync();
            int bytesRead = await stream.ReadAsync(memoryOwner.Memory[..(int)contentLength]);

            if (bytesRead < contentLength)
            {
                Console.WriteLine("Ошибка: не все данные загружены");
                return null;
            }

            // 5. Кешируем в памяти
            _memoryCache.Set(
                cacheKey,
                memoryOwner,
                new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(30),
                    Size = 1,
                }
            );

            // 6. Сохраняем на диск
            await using var file = File.OpenWrite(filePath);
            await file.WriteAsync(memoryOwner.Memory[..(int)contentLength]);

            return CreateBitmapFromMemory(memoryOwner.Memory);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки тайла {cacheKey}: {ex.Message}");
            return null;
        }
    }

    private Bitmap CreateBitmapFromMemory(ReadOnlyMemory<byte> memory)
    {
        return new Bitmap(new MemoryStream(memory.ToArray()));
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
    }
}

public class MapCanvas2 : Control
{
    private const int TileSize = 256;
    private readonly BingTileLoader _tileLoader;
    private readonly Dictionary<(int, int, int), Bitmap?> _tiles = new();
    private int _zoom = 5;
    private Point _offset = new Point(0, 0);
    private Point _lastMousePosition;
    private int _centerTileX;
    private int _centerTileY;

    public MapCanvas2()
    {
        _tileLoader = new BingTileLoader(
            "Anqg-XzYo-sBPlzOWFHIcjC3F8s17P_O7L4RrevsHVg4fJk6g_eEmUBphtSn4ySg"
        );
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerWheelChanged += OnPointerWheelChanged;

        // Центрируем карту по Москве (55.7558, 37.6173)
        CenterMap(55.7558, 37.6173);
    }

    private void CenterMap(double latitude, double longitude)
    {
        BingMapsHelper.LatLongToTileXY(
            latitude,
            longitude,
            _zoom,
            out _centerTileX,
            out _centerTileY
        );

        // Центрируем относительно окна
        _offset = new Point(
            -(TileSize * _centerTileX) + (Bounds.Width / 2),
            -(TileSize * _centerTileY) + (Bounds.Height / 2)
        );

        LoadVisibleTiles();
    }

    private async void LoadVisibleTiles()
    {
        int tilesX = (int)Math.Ceiling(Bounds.Width / TileSize) + 2;
        int tilesY = (int)Math.Ceiling(Bounds.Height / TileSize) + 2;

        int startX = _centerTileX - (tilesX / 2);
        int startY = _centerTileY - (tilesY / 2);

        var newTiles = new List<Task>();

        for (int x = startX; x < startX + tilesX; x++)
        {
            for (int y = startY; y < startY + tilesY; y++)
            {
                var key = (x, y, _zoom);
                if (!_tiles.ContainsKey(key))
                {
                    var tileTask = LoadTileAsync(x, y, _zoom);
                    newTiles.Add(tileTask);
                }
            }
        }

        await Task.WhenAll(newTiles);
        InvalidateVisual();
    }

    private async Task LoadTileAsync(int tileX, int tileY, int zoom)
    {
        var key = (tileX, tileY, zoom);
        var tile = await _tileLoader.LoadTileAsync(tileX, tileY, zoom);
        _tiles[key] = tile;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        foreach (var ((x, y, z), bitmap) in _tiles)
        {
            if (bitmap != null)
            {
                double px = (x * TileSize) + _offset.X;
                double py = (y * TileSize) + _offset.Y;

                if (
                    px + TileSize < 0
                    || py + TileSize < 0
                    || px > Bounds.Width
                    || py > Bounds.Height
                )
                {
                    continue;
                }

                context.DrawImage(
                    bitmap,
                    new Rect(0, 0, TileSize, TileSize),
                    new Rect(px, py, TileSize, TileSize)
                );
            }
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _lastMousePosition = e.GetPosition(this);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            Point currentPosition = e.GetPosition(this);
            _offset += currentPosition - _lastMousePosition;
            _lastMousePosition = currentPosition;

            UpdateCenterTile();
            LoadVisibleTiles();
            InvalidateVisual();
        }
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        double oldCenterLat,
            oldCenterLon;
        TileXYToLatLong(_centerTileX, _centerTileY, _zoom, out oldCenterLat, out oldCenterLon);

        if (e.Delta.Y > 0 && _zoom < 19)
        {
            _zoom++;
        }
        else if (e.Delta.Y < 0 && _zoom > 1)
        {
            _zoom--;
        }

        _tiles.Clear();
        CenterMap(oldCenterLat, oldCenterLon);
    }

    private void UpdateCenterTile()
    {
        int newCenterX = (int)((-(_offset.X - (Bounds.Width / 2))) / TileSize);
        int newCenterY = (int)((-(_offset.Y - (Bounds.Height / 2))) / TileSize);

        if (_centerTileX != newCenterX || _centerTileY != newCenterY)
        {
            _centerTileX = newCenterX;
            _centerTileY = newCenterY;
            LoadVisibleTiles();
        }
    }

    private void TileXYToLatLong(
        int tileX,
        int tileY,
        int zoom,
        out double latitude,
        out double longitude
    )
    {
        double n = Math.PI - ((2.0 * Math.PI * tileY) / Math.Pow(2.0, zoom));
        latitude = (180.0 / Math.PI) * Math.Atan(Math.Sinh(n));
        longitude = (tileX / Math.Pow(2.0, zoom) * 360.0) - 180.0;
    }
}
