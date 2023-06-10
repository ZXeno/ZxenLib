namespace ZxenLib.Physics.Collision.Shapes;

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Common;

/// This describes the motion of a body/shape for TOI computation.
/// Shapes are defined with respect to the body origin, which may
/// no coincide with the center of mass. However, to support dynamics
/// we must interpolate the center of mass position.
public struct Sweep
{
    /// <summary>
    /// Get the interpolated transform at a specific time.
    /// @param beta is a factor in [0,1], where 0 indicates alpha0.
    /// https://fgiesen.wordpress.com/2012/08/15/linear-interpolation-past-present-and-future/
    /// </summary>
    /// <param name="xf"></param>
    /// <param name="beta"></param>
    public void GetTransform(out Transform xf, float beta)
    {
        Vector2 position = (1.0f - beta) * this.C0 + beta * this.C;
        float angle = (1.0f - beta) * this.A0 + beta * this.A;
        xf = new Transform(position, angle);

        // Shift to origin
        xf.Position -= MathUtils.Mul(xf.Rotation, this.LocalCenter);
    }

    /// Advance the sweep forward, yielding a new initial state.
    /// @param alpha the new initial time.
    public void Advance(float alpha)
    {
        Debug.Assert(this.Alpha0 < 1.0f);
        float beta = (alpha - this.Alpha0) / (1.0f - this.Alpha0);
        this.C0 += beta * (this.C - this.C0);
        this.A0 += beta * (this.A - this.A0);
        this.Alpha0 = alpha;
    }

    /// Normalize the angles.
    public void Normalize()
    {
        const float twoPi = 2.0f * Settings.Pi;
        float d = twoPi * (float)Math.Floor(this.A0 / twoPi);
        this.A0 -= d;
        this.A -= d;
    }

    /// <summary>
    /// local center of mass position
    /// </summary>
    public Vector2 LocalCenter;

    /// <summary>
    /// center world positions
    /// </summary>
    public Vector2 C0, C;

    /// <summary>
    /// world angles
    /// </summary>
    public float A0, A;

    /// Fraction of the current time step in the range [0,1]
    /// c0 and a0 are the positions at alpha0.
    public float Alpha0;
}