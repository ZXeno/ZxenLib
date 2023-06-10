namespace ZxenLib.Physics.Common;

using System;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;

/// Rotation
public struct Rotation
{
    /// Sine and cosine
    public float Sin;

    public float Cos;

    public Rotation(float sin, float cos)
    {
        this.Sin = sin;
        this.Cos = cos;
    }

    /// Initialize from an angle in radians
    public Rotation(float angle)
    {
        // TODO_ERIN optimize
        this.Sin = (float) Math.Sin(angle);
        this.Cos = (float) Math.Cos(angle);
    }

    /// Set using an angle in radians.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(float angle)
    {
        // TODO_ERIN optimize
        this.Sin = (float) Math.Sin(angle);
        this.Cos = (float) Math.Cos(angle);
    }

    /// Set to the identity rotation
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetIdentity()
    {
        this.Sin = 0.0f;
        this.Cos = 1.0f;
    }

    /// Get the angle in radians
    public float Angle => (float) Math.Atan2(this.Sin, this.Cos);

    /// Get the x-axis
    public Vector2 GetXAxis()
    {
        return new Vector2(this.Cos, this.Sin);
    }

    /// Get the u-axis
    public Vector2 GetYAxis()
    {
        return new Vector2(-this.Sin, this.Cos);
    }
}