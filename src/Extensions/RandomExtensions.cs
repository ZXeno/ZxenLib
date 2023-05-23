namespace ZxenLib.Extensions;

using System;

public static class RandomExtensions
{
    /// <summary>
    /// Gets a float value within a random range. Not precise, not fast. Use sparingly.<br/>
    /// <para>This works by taking the float numbers, multiplying them by the precision parameter,
    /// then calling Random.Next(min, max), and return the result divided by precision.</para>
    /// </summary>
    /// <param name="random">The instance of <see cref="Random"/>.</param>
    /// <param name="min">The inclusive minimum boundary of the range.</param>
    /// <param name="max">The inclusive upper boundary of the range.</param>
    /// <param name="precision">Divisor.</param>
    /// <returns></returns>
    public static float RangeSingle(this Random random, float min, float max, float precision = 1000f)
    {
        int lowerBound = (int)(min * precision);
        int upperBound = (int)(max * precision + 1);
        int retVal = random.Next(lowerBound, upperBound);
        return retVal / precision;
    }
}