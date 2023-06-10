namespace ZxenLib.Physics.Collision;

using System;
using System.Diagnostics.Contracts;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;
using Collider;
using Common;

/// <summary>
///     An axis aligned bounding box.
/// </summary>
public struct AABB
{
    /// <summary>
    ///     the lower vertex
    /// </summary>
    public Vector2 LowerBound;

    /// <summary>
    ///     the upper vertex
    /// </summary>
    public Vector2 UpperBound;

    public AABB(in Vector2 lowerBound, in Vector2 upperBound)
    {
        this.LowerBound = lowerBound;
        this.UpperBound = upperBound;
    }

    /// <summary>
    ///     Verify that the bounds are sorted.
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Pure]
    public bool IsValid()
    {
        Vector2 d = this.UpperBound - this.LowerBound;
        bool valid = d.X >= 0.0f && d.Y >= 0.0f;
        valid = valid && this.LowerBound.IsValid() && this.UpperBound.IsValid();
        return valid;
    }

    /// <summary>
    ///     Get the center of the AABB.
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Pure]
    public Vector2 GetCenter()
    {
        return 0.5f * (this.LowerBound + this.UpperBound);
    }

    /// <summary>
    ///     Get the extents of the AABB (half-widths).
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Pure]
    public Vector2 GetExtents()
    {
        return 0.5f * (this.UpperBound - this.LowerBound);
    }

    /// <summary>
    ///     Get the perimeter length
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Pure]
    public float GetPerimeter()
    {
        float wx = this.UpperBound.X - this.LowerBound.X;
        float wy = this.UpperBound.Y - this.LowerBound.Y;
        return wx + wx + wy + wy;
    }

    public bool RayCast(out RayCastOutput output, in RayCastInput input)
    {
        output = default;
        float tmin = -Settings.MaxFloat;
        float tmax = Settings.MaxFloat;

        Vector2 p = input.P1;
        Vector2 d = input.P2 - input.P1;
        Vector2 absD = MathExtensions.Vector2Abs(d);

        Vector2 normal = new Vector2();

        {
            if (absD.X < Settings.Epsilon)
            {
                // Parallel.
                if (p.X < this.LowerBound.X || this.UpperBound.X < p.X)
                {
                    return false;
                }
            }
            else
            {
                float invD = 1.0f / d.X;
                float t1 = (this.LowerBound.X - p.X) * invD;
                float t2 = (this.UpperBound.X - p.X) * invD;

                // Sign of the normal vector.
                float s = -1.0f;

                if (t1 > t2)
                {
                    MathUtils.Swap(ref t1, ref t2);
                    s = 1.0f;
                }

                // Push the min up
                if (t1 > tmin)
                {
                    normal.SetZero();
                    normal.X = s;
                    tmin = t1;
                }

                // Pull the max down
                tmax = Math.Min(tmax, t2);

                if (tmin > tmax)
                {
                    return false;
                }
            }
        }
        {
            if (absD.Y < Settings.Epsilon)
            {
                // Parallel.
                if (p.Y < this.LowerBound.Y || this.UpperBound.Y < p.Y)
                {
                    return false;
                }
            }
            else
            {
                float invD = 1.0f / d.Y;
                float t1 = (this.LowerBound.Y - p.Y) * invD;
                float t2 = (this.UpperBound.Y - p.Y) * invD;

                // Sign of the normal vector.
                float s = -1.0f;

                if (t1 > t2)
                {
                    MathUtils.Swap(ref t1, ref t2);
                    s = 1.0f;
                }

                // Push the min up
                if (t1 > tmin)
                {
                    normal.SetZero();
                    normal.Y = s;
                    tmin = t1;
                }

                // Pull the max down
                tmax = Math.Min(tmax, t2);

                if (tmin > tmax)
                {
                    return false;
                }
            }
        }

        // Does the ray start inside the box?
        // Does the ray intersect beyond the max fraction?
        if (tmin < 0.0f || input.MaxFraction < tmin)
        {
            return false;
        }

        // Intersection.
        output = new RayCastOutput {Fraction = tmin, Normal = normal};

        return true;
    }

    public static void Combine(in AABB left, in AABB right, out AABB aabb)
    {
        aabb = new AABB(
            Vector2.Min(left.LowerBound, right.LowerBound),
            Vector2.Max(left.UpperBound, right.UpperBound));
    }

    /// <summary>
    ///     Combine an AABB into this one.
    /// </summary>
    /// <param name="aabb"></param>
    public void Combine(in AABB aabb)
    {
        this.LowerBound = Vector2.Min(this.LowerBound, aabb.LowerBound);
        this.UpperBound = Vector2.Max(this.UpperBound, aabb.UpperBound);
    }

    /// <summary>
    ///     Combine two AABBs into this one.
    /// </summary>
    /// <param name="aabb1"></param>
    /// <param name="aabb2"></param>
    public void Combine(in AABB aabb1, in AABB aabb2)
    {
        this.LowerBound = Vector2.Min(aabb1.LowerBound, aabb2.LowerBound);
        this.UpperBound = Vector2.Max(aabb1.UpperBound, aabb2.UpperBound);
    }

    /// <summary>
    ///     Does this aabb contain the provided AABB.
    /// </summary>
    /// <param name="aabb">the provided AABB</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Pure]
    public bool Contains(in AABB aabb)
    {
        return this.LowerBound.X <= aabb.LowerBound.X
               && this.LowerBound.Y <= aabb.LowerBound.Y
               && aabb.UpperBound.X <= this.UpperBound.X
               && aabb.UpperBound.Y <= this.UpperBound.Y;
    }
}