using System;

namespace EasyWeapons.Demo.Math;

public enum Direction
{
    Forward,
    Backward,
    Left,
    Right,
    Up,
    Down
}

public static class DirectionExtensions
{
    public static readonly Exception UnknownDirectionException = new ArgumentException($"Unknown {nameof(Direction)}");

    public static Vector3 GetNormal(this Direction direction)
    {
        return direction switch
        {
            Direction.Forward => Vector3.Forward,
            Direction.Backward => Vector3.Backward,
            Direction.Left => Vector3.Left,
            Direction.Right => Vector3.Right,
            Direction.Up => Vector3.Up,
            Direction.Down => Vector3.Down,
            _ => throw UnknownDirectionException
        };
    }
    public static Direction GetOpposite(this Direction direction)
    {
        return direction switch
        {
            Direction.Forward => Direction.Backward,
            Direction.Backward => Direction.Forward,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            _ => throw UnknownDirectionException
        };
    }
}
