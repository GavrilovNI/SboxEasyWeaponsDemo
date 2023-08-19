using System;
using System.Numerics;

namespace EasyWeapons.Demo.Math;

public enum AxisDirection
{
    Positive,
    Negative
}

public static class AxisDirectionExtensions
{
    public static readonly Exception UnknownAxisDirectionException = new System.ArgumentException($"Unknown {nameof(AxisDirection)}");

    public static AxisDirection GetOpposite(this AxisDirection axis)
    {
        return axis switch
        {
            AxisDirection.Positive => AxisDirection.Negative,
            AxisDirection.Negative => AxisDirection.Positive,
            _ => throw UnknownAxisDirectionException
        };
    }

    public static int GetNormal(this AxisDirection axis)
    {
        return axis switch
        {
            AxisDirection.Positive => 1,
            AxisDirection.Negative => -1,
            _ => throw UnknownAxisDirectionException
        };
    }

    public static Vector3 GetLimits(this BBox bBox, AxisDirection axisDirection)
    {
        return axisDirection switch
        {
            AxisDirection.Positive => bBox.Maxs,
            AxisDirection.Negative => bBox.Mins,
            _ => throw UnknownAxisDirectionException
        };
    }

    public static void SetLimitsOn(this AxisDirection axisDirection, ref BBox bBox, Vector3 limits)
    {
        switch(axisDirection)
        {
            case AxisDirection.Positive:
                bBox.Maxs = limits;
                return;
            case AxisDirection.Negative:
                bBox.Mins = limits;
                return;
        }

        throw UnknownAxisDirectionException;
    }
}
