namespace ZxenLib.Extensions;

using System;
using Microsoft.Xna.Framework;

public static class Vector2Extensions
{
    public static Vector2 Slerp(this Vector2 from, Vector2 to, float step)
    {
        if (step == 0)
        {
            return from;
        }

        if (from == to || step == 1)
        {
            return to;
        }

        double theta = Math.Acos(Vector2.Dot(from, to));
        if (theta == 0)
        {
            return to;
        }

        double sinTheta = Math.Sin(theta);
        return (float)(Math.Sin((1 - step) * theta) / sinTheta) * from + (float)(Math.Sin(step * theta) / sinTheta) * to;
    }
}