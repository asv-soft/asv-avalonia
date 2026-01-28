using Asv.Common;

namespace Asv.Avalonia.GeoMap;

public enum MoveDirection
{
    Up,
    Down,
    Forward,
    Backward,
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
            MoveDirection.Forward => 0,
            MoveDirection.Right => 90,
            MoveDirection.Backward => 180,
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
        switch (moveDirection)
        {
            case MoveDirection.Up:
            {
                return from.AddAltitude(distanceInSi);
            }
            case MoveDirection.Down:
            {
                return from.AddAltitude(-distanceInSi);
            }
            default:
            {
                var deg = DirectionToDegrees(moveDirection);
                var normalizedDeg = ((deg % 360) + 360) % 360;
                return from.RadialPoint(distanceInSi, normalizedDeg);
            }
        }
    }
}
