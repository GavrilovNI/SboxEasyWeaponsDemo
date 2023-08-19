using System;

namespace EasyWeapons.Demo.Math;

public enum Axis
{
    X,
    Y,
    Z
}

public static class AxisExtensions
{
    public static readonly Exception UnknownAxisException = new System.ArgumentException($"Unknown {nameof(Axis)}");

    public static Direction GetDirection(this Axis axis, AxisDirection axisDirection)
    {
        if(axisDirection == AxisDirection.Positive)
        {
            return axis switch
            {
                Axis.X => Direction.Forward,
                Axis.Y => Direction.Left,
                Axis.Z => Direction.Up,
                _ => throw UnknownAxisException
            };
        }
        else if(axisDirection == AxisDirection.Negative)
        {
            return axis switch
            {
                Axis.X => Direction.Backward,
                Axis.Y => Direction.Right,
                Axis.Z => Direction.Down,
                _ => throw UnknownAxisException
            };
        }

        throw AxisDirectionExtensions.UnknownAxisDirectionException;
    }

    public static float GetComponent(this Vector3 vector, Axis axis)
    {
        return axis switch
        {
            Axis.X => vector.x,
            Axis.Y => vector.y,
            Axis.Z => vector.z,
            _ => throw UnknownAxisException
        };
    }

    public static Vector3 WithComponent(this Vector3 vector, Axis axis, float value)
    {
        return axis switch
        {
            Axis.X => vector.WithX(value),
            Axis.Y => vector.WithY(value),
            Axis.Z => vector.WithZ(value),
            _ => throw UnknownAxisException
        };
    }

    public static void SetOn(this Axis axis, ref Vector3 vector, float value)
    {
        switch(axis)
        {
            case Axis.X:
                vector.x = value;
                return;
            case Axis.Y:
                vector.y = value;
                return;
            case Axis.Z:
                vector.z = value;
                return;
        }

        throw UnknownAxisException;
    }
}
