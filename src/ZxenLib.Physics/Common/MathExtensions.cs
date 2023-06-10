namespace ZxenLib.Physics.Common;

using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;

public static class MathExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValid(in this Vector2 vector2)
    {
        return !float.IsInfinity(vector2.X) && !float.IsInfinity(vector2.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValid(this float x)
    {
        return !float.IsInfinity(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetZero(ref this Vector2 vector2)
    {
        vector2.X = 0.0f;
        vector2.Y = 0.0f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set(ref this Vector2 vector2, float x, float y)
    {
        vector2.X = x;
        vector2.Y = y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetZero(ref this Vector3 vector3)
    {
        vector3.X = 0.0f;
        vector3.Y = 0.0f;
        vector3.Z = 0.0f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set(ref this Vector3 vector3, float x, float y, float z)
    {
        vector3.X = x;
        vector3.Y = y;
        vector3.Z = z;
    }

    /// Convert this vector into a unit vector. Returns the length.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Normalize(ref Vector2 vector2)
    {
        float length = vector2.Length();
        if (length < Settings.Epsilon)
        {
            return 0.0f;
        }

        float invLength = 1.0f / length;
        vector2.X *= invLength;
        vector2.Y *= invLength;

        return length;
    }

    /// <summary>
    ///  Get the skew vector such that dot(skew_vec, other) == cross(vec, other)
    /// </summary>
    /// <param name="vector2"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Skew(ref this Vector2 vector2)
    {
        return new Vector2(-vector2.Y, vector2.X);
    }

    /// <summary>Returns a vector whose elements are the absolute values of each of the specified vector's elements.</summary>
    /// <param name="value">A vector.</param>
    /// <returns>The absolute value vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Vector2Abs(Vector2 value)
    {
        return new Vector2(
            MathF.Abs(value.X),
            MathF.Abs(value.Y)
        );
    }
}