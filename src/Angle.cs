namespace ZxenLib;

using System;
using Microsoft.Xna.Framework;

/// <summary>
/// Implementation of Angle and its relative methods and information.
/// </summary>
public class Angle
{
    private float degrees;

    /// <summary>
    /// Initializes a new instance of the <see cref="Angle"/> class.
    /// </summary>
    public Angle()
    {
        this.degrees = 0;
        this.Direction = new Vector2(0, 0);
    }

    /// <summary>
    /// Gets or sets the 2D Direction Vector.
    /// </summary>
    public Vector2 Direction { get; set; }

    /// <summary>
    /// Gets the Angle in degrees.
    /// </summary>
    public float Degrees
    {
        get => this.degrees;
        private set
        {
            this.degrees = value;
            this.Direction = new Vector2((float)Math.Cos(this.Degrees), (float)Math.Sin(this.Degrees));
        }
    }

    /// <summary>
    /// Gets the angle in radians.
    /// </summary>
    public float Radians => MathHelper.ToRadians(this.Degrees);

    /// <summary>
    /// Adds passed rotation amount in radians to object's rotation.
    /// Non-radian values are undefined.
    /// </summary>
    /// <param name="addedRotationDegrees">The amount of rotation in radians to be appended.</param>
    public void Rotate(float addedRotationDegrees)
    {
        if (this.Degrees + addedRotationDegrees < 0)
        {
            this.Degrees += addedRotationDegrees + 360;
            return;
        }

        if (this.Degrees + addedRotationDegrees > 360)
        {
            this.Degrees += addedRotationDegrees - 360;
            return;
        }

        this.Degrees += addedRotationDegrees;
    }

    /// <summary>
    /// Directly sets object's rotation. Value must be in degrees.
    /// Values clamped between 0 and 360;.
    /// </summary>
    /// <param name="degreeRotation">The rotation to set in degrees.</param>
    public void SetRotation(float degreeRotation)
    {
        this.Degrees = MathHelper.Clamp(degreeRotation, 0, 360);
    }
}