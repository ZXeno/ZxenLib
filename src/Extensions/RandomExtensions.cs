namespace ZxenLib.Extensions;

using System;

public static class RandomExtensions
{
    /// <summary>
    /// Gets a float value within a random range. Not precise, not fast. Use sparingly.<br/>
    /// <para>generating a random double between 0.0 and 1.0, scaling it to the desired range
    /// by multiplying it with the difference between the maximum and minimum values (`maxValue - minValue`),
    /// and then shifting the range to start at the minimum value (+ minValue).</para>
    /// </summary>
    /// <param name="random">The instance of <see cref="Random"/>.</param>
    /// <param name="min">The inclusive minimum boundary of the range.</param>
    /// <param name="max">The inclusive upper boundary of the range.</param>
    /// <returns></returns>
    public static float RangeSingle(this Random random, float min, float max)
    {
        return (float)(random.NextDouble() * (min - max) + min);
    }
}