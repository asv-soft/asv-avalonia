using Avalonia;

namespace Asv.Avalonia.Map;

public static class PointExtensions
{
    // Вычисляет длину вектора (расстояние от начала координат до точки)
    public static double Length(this Point point)
    {
        return Math.Sqrt(point.X * point.X + point.Y * point.Y);
    }

    // Нормализует вектор (приводит его к единичной длине)
    public static Point Normalize(this Point point)
    {
        var length = point.Length();
        if (length < double.Epsilon) // Проверка на нулевую длину, чтобы избежать деления на 0
        {
            return new Point(0, 0);
        }
        return new Point(point.X / length, point.Y / length);
    }

    // Опционально: вычисление квадрата длины (быстрее, чем Length, если не нужен корень)
    public static double LengthSquared(this Point point)
    {
        return point.X * point.X + point.Y * point.Y;
    }
}
