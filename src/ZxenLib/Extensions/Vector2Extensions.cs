namespace ZxenLib.Extensions;

using System;
using System.Runtime.CompilerServices;
using Infrastructure;
using Microsoft.Xna.Framework;

public static class Vector2Extensions
{
    public static Vector2 Slerp(this Vector2 from, Vector2 to, float step)
    {
        if (step == 0)
        {
            return from;
        }

        if (from.Compare(to) || step.Compare(1))
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

    /// <summary>
    /// Impure method that rotates a <see cref="Vector2"/> around an origin by the provided degrees.
    /// </summary>
    /// <param name="vec">The <see cref="Vector2"/> being rotated.</param>
    /// <param name="origin">The origin of which the vector is being rotated around.</param>
    /// <param name="rotation">How far to rotate the <see cref="Vector2"/>. If <see cref="useDegrees"/> is false, provide radians instead.</param>
    /// <param name="useDegrees">Flag indicating of the <see cref="rotation"/> parameter is in degrees or radians. True by default, specifying that <see cref="rotation"/> is degrees.</param>
    public static Vector2 Rotate(this Vector2 vec, Vector2 origin, double rotation, bool useDegrees = true)
    {
        double x = vec.X - origin.X;
        double y = vec.Y - origin.Y;
        rotation = useDegrees ? MathHelper.ToRadians((float)rotation) : rotation;

        double cos = Math.Cos(rotation);
        double sin = Math.Sin(rotation);

        vec.X = (float)((x * cos) - (y * sin)) + origin.X;
        vec.Y = (float)((x * sin) + (y * cos)) + origin.Y;

        return vec;
    }

    /// <summary>
    /// Compares two <see cref="Vector2"/> by checking if their corresponding values are within epsilon.
    /// <para><see cref="ZxMath.Compare(float,float,float)"/> for details.</para>
    /// </summary>
    /// <param name="v1">The first <see cref="Vector2"/> to compare.</param>
    /// <param name="v2">The second <see cref="Vector2"/> to compare.</param>
    /// <param name="epsilon">The margin of error. Default value is <see cref="float.MinValue"/>.</param>
    /// <returns>True if v1.X and v2.X are within epsilon, and if v1.Y and v2.Y are also within epsilon.</returns>
    public static bool Compare(this Vector2 v1, Vector2 v2, float epsilon = float.MinValue)
    {
        return ZxMath.Compare(v1, v2, epsilon);
    }

    /// <summary>
    /// Perform the cross product on a vector and a scalar. In 2D this produces a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="s"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Cross(Vector2 a, float s)
    {
        return new Vector2(s * a.Y, -s * a.X);
    }
}