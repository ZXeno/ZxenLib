namespace ZxenLib.Extensions;

using System;

public static class RandomExtensions
{
    /// <summary>
    /// Gets a random float in a given range.<br/><para>First, gets an integer value
    /// between the min and max value. Then gets a random double (between 0.0 and 1.0)
    /// and adds these two numbers together when returning.</para>
    /// </summary>
    /// <param name="random">The instance of <see cref="Random"/>.</param>
    /// <param name="min">The inclusive minimum boundary of the range.</param>
    /// <param name="max">The inclusive upper boundary of the range.</param>
    /// <returns></returns>
    public static float RangeSingle(this Random random, float min, float max)
    {
        if (min == max)
        {
            return min;
        }

        if (min > max)
        {
            throw new ArgumentException($"{nameof(min)} cannot be greater than {nameof(max)}");
        }

        int wholeNumber = random.Next((int)min, (int)max);
        float irrationalNumber = random.NextSingle();
        irrationalNumber = wholeNumber < 0 ? -irrationalNumber : irrationalNumber;
        return wholeNumber + irrationalNumber;
    }
}