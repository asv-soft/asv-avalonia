using Asv.Common;

namespace Asv.Avalonia;

public enum MoveDirection
{
    Up,
    Down,
    Left,
    Right,
    UpRight,
    UpLeft,
    DownRight,
    DownLeft,
}

public static class GeoPointMoveHelper
{
    private static double DirectionToDegrees(MoveDirection d)
    {
        return d switch
        {
            MoveDirection.Up => 0,
            MoveDirection.Right => 90,
            MoveDirection.Down => 180,
            MoveDirection.Left => 270,
            MoveDirection.UpRight => 45,
            MoveDirection.UpLeft => 315,
            MoveDirection.DownRight => 135,
            MoveDirection.DownLeft => 225,
            _ => 0,
        };
    }

    public static GeoPoint Step(GeoPoint from, double distanceInSi, MoveDirection moveDirection)
    {
        var deg = DirectionToDegrees(moveDirection);
        var normalizedDeg = ((deg % 360) + 360) % 360;
        return from.RadialPoint(distanceInSi, normalizedDeg);
    }
}
