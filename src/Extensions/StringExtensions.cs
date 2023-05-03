namespace ZxenLib.Extensions;

using System;
using System.Runtime.CompilerServices;

public static class StringExtensions
{
    public static void ThrowIfNull(this string str, [CallerArgumentExpression("paramName")] string? paramName = null )
    {
        if (str == null)
        {
            throw new ArgumentNullException(paramName);
        }
    }

    public static void ThrowIfNullOrEmpty(this string str, [CallerArgumentExpression("paramName")] string? paramName = null)
    {
        if (string.IsNullOrEmpty(str))
        {
            throw new ArgumentNullException(paramName);
        }
    }

    public static void ThrowIfNullOrWhitespace(this string str, [CallerArgumentExpression("paramName")] string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            throw new ArgumentNullException(paramName);
        }
    }
}