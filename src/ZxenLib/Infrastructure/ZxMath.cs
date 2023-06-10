namespace ZxenLib.Infrastructure;

using System;
using Microsoft.Xna.Framework;

public static class ZxMath
{
    /// <summary>
    /// Compares two float values, x and y, for equality within a given relative tolerance.
    /// </summary>
    /// <param name="x">The first value being checked.</param>
    /// <param name="y">The second value being compared.</param>
    /// <param name="epsilon">The margin of error. Default value is <see cref="DefaultEpsilon"/>.</param>
    /// <returns>"Good enough" if the absolute value of `x - y` is within the scale of `epsilon` tolerance based on the values.</returns>
    public static bool Compare(float x, float y, float epsilon = float.MinValue)
    {
        return Math.Abs(x - y) <= Math.Abs(epsilon);
    }

    /// <summary>
    /// Compares two <see cref="Vector2"/> to see if they are approximately equal using the <see cref="Compare(float,float,float)"/>
    /// function perform a check within their epsilon. <para>See <see cref="Compare(float,float,float)"/> documentation for details.</para>
    /// </summary>
    /// <param name="v1">The first <see cref="Vector2"/> to compare.</param>
    /// <param name="v2">The second <see cref="Vector2"/> to compare.</param>
    /// <param name="epsilon">The margin of error. Default value is <see cref="float.MinValue"/>.</param>
    /// <returns>True if v1.X and v2.X are within epsilon, and if v1.Y and v2.Y are also within epsilon.</returns>
    public static bool Compare(Vector2 v1, Vector2 v2, float epsilon = float.MinValue)
    {
        return Compare(v1.X, v2.X, epsilon) && Compare(v1.Y, v2.Y, epsilon);
    }

    /// <summary>
    /// Rotates a pair of floating point coordinates around an origin pair of floating point coordinates by the provided degrees.
    /// </summary>
    /// <param name="pX">X coordinate of point.</param>
    /// <param name="pY">Y coordinate of point.</param>
    /// <param name="oX">X coordinate of ORIGIN.</param>
    /// <param name="oY">Y coordinate of ORIGIN.</param>
    /// <param name="rotation">How far to rotate the <see cref="Vector2"/>. If <see cref="useDegrees"/> is false, provide radians instead.</param>
    /// <param name="useDegrees">Flag indicating of the <see cref="rotation"/> parameter is in degrees or radians. True by default, specifying that <see cref="rotation"/> is degrees.</param>
    public static Span<float> RotateCoords(float pX, float pY, float oX, float oY, double rotation, bool useDegrees = true)
    {
        double x = pX - oX;
        double y = pY - oY;
        rotation = useDegrees ? MathHelper.ToRadians((float)rotation) : rotation;

        double cos = Math.Cos(rotation);
        double sin = Math.Sin(rotation);

        pX = (float)((x * cos) - (y * sin)) + oX;
        pY = (float)((x * sin) + (y * cos)) + oY;

        return new Span<float>(new[] { pX, pY });
    }
}