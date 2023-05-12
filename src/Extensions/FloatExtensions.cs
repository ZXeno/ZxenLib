namespace ZxenLib.Extensions;

public static class FloatExtensions
{
    /// <summary>
    /// Determines if a float value is within a given threshold between the negative and positive value of the limit.
    /// Useful for determining if a tiny float value is within a margin of error without rewriting the check.
    /// </summary>
    /// <param name="val"></param>
    /// <param name="threshHold"></param>
    /// <returns></returns>
    public static bool WithinThreshold(this float val, float threshHold)
    {
        return val <= threshHold && val >= -threshHold;
    }
}