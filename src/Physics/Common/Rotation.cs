namespace ZxenLib.Physics.Common;

using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

public class  Rotation
{
    public static Rotation Identity = new();

    private float sin;
    private float cos;
    private float angle;

    /// <summary>
    /// Creates a new instance of teh <see cref="Rotation"/> class.
    /// </summary>
    public Rotation()
    {
        this.angle = 0f;
        this.sin = 0.0f;
        this.cos = 1.0f;
    }

    /// <summary>
    /// Creates a new instance of teh <see cref="Rotation"/> class.
    /// </summary>
    /// <param name="angle">The angle in radians.</param>
    public Rotation(float angle)
    {
        this.angle = angle;
        this.sin = (float)Math.Sin(angle);
        this.cos = (float)Math.Cos(angle);
    }

    /// <summary>
    /// Creates a new
    /// </summary>
    /// <param name="sin"></param>
    /// <param name="cos"></param>
    public Rotation(float sin, float cos)
    {
        this.sin = sin;
        this.cos = cos;
        this.angle = (float)Math.Atan2(sin, cos);
    }

    /// <summary>
    /// Sets the angle.
    /// </summary>
    /// <param name="angle">The angle to set. Defaults to radians.</param>
    /// <param name="useDegrees">If this is true, will convert from degrees to radians.</param>
    public void Set(float angle, bool useDegrees = false)
    {
        angle = useDegrees ? MathHelper.ToRadians(angle) : angle;

        this.sin = (float)Math.Sin(angle);
        this.cos = (float)Math.Cos(angle);
    }

    /// <summary>
    /// Sets the identity of the rotation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetIdentity()
    {
        this.angle = 0f;
        this.sin = 0f;
        this.cos = 1f;
    }

    /// <summary>
    /// Gets the angle in Radians.
    /// </summary>
    public float Angle => (float) Math.Atan2(this.sin, this.cos);

    /// Sine and cosine
    public float Sin
    {
        get => this.sin;
        set => this.sin = value;
    }

    public float Cos
    {
        get => this.cos;
        set => this.cos = value;
    }


    /// <summary>
    /// Gets the X axis of the rotation.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetXAxis()
    {
        return new Vector2(this.cos, this.sin);
    }

    /// <summary>
    /// Gets the Y axis of the rotation.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetYAxis()
    {
        return new Vector2(-this.sin, this.cos);
    }
}