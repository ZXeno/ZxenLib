namespace ZxenLib.Extensions;

using System;

public static class FloatExtensions
{
    public const float DefaultEpsilon = .000001f;

    /// <summary>
    /// Checks if the absolute value of the given float x is less than or equal to the absolute value of a provided threshold
    /// </summary>
    /// <param name="x">The value being checked.</param>
    /// <param name="epsilon">The margin of error. Default value is <see cref="DefaultEpsilon"/>.</param>
    /// <returns>True if the absolute value of `x` is within the absolute value of `epsilon`.</returns>
    public static bool WithinThreshold(this float x, float epsilon = DefaultEpsilon)
    {
        return Math.Abs(x) <= Math.Abs(epsilon);
    }

    /// <summary>
    /// Compares two float values, x and y, for equality within a given relative tolerance.
    /// </summary>
    /// <param name="x">The first value being checked.</param>
    /// <param name="y">The second value being compared.</param>
    /// <param name="epsilon">The margin of error. Default value is <see cref="DefaultEpsilon"/>.</param>
    /// <returns>"Good enough" if the absolute value of `x - y` is within the scale of `epsilon` tolerance based on the values.</returns>
    public static bool Compare(this float x, float y, float epsilon = DefaultEpsilon)
    {
        return Math.Abs(x - y) <= Math.Abs(epsilon);
    }

    /// <summary>
    /// Squares the value.
    /// </summary>
    /// <param name="val">The value to square</param>
    /// <returns><see cref="float"/></returns>
    public static float Sqr(this float val)
    {
        return val * val;
    }
}