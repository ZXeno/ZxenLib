namespace ZxenLib;

using System;
using Microsoft.Xna.Framework;

/// <summary>
/// Implementation of Angle and its relative methods and information.
/// </summary>
public class Angle
{
    private float degrees;
    private float cos;
    private float sin;

    /// <summary>
    /// Initializes a new instance of the <see cref="Angle"/> class.
    /// </summary>
    public Angle()
    {
        this.degrees = 0;
        this.cos = 0;
        this.sin = 0;
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
            this.cos = (float)Math.Cos(this.Degrees);
            this.sin = (float)Math.Sin(this.Degrees);
            this.Direction = new Vector2(this.cos, this.sin);
        }
    }

    /// <summary>
    /// Gets the angle in radians.
    /// </summary>
    public float Radians => MathHelper.ToRadians(this.Degrees);

    /// <summary>
    /// The cosine of the angle.
    /// </summary>
    public float Cos => this.cos;

    /// <summary>
    /// The sine of the angle.
    /// </summary>
    public float Sin => this.sin;

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
    /// </summary>
    /// <param name="degreeRotation">The rotation to set in degrees.</param>
    public void SetRotation(float degreeRotation)
    {
        if (degreeRotation > 360)
        {
            degreeRotation -= 360;
        }

        if (degreeRotation < 0)
        {
            degreeRotation += 360;
        }

        this.Degrees = MathHelper.Clamp(degreeRotation, 0, 360);
    }

    public static bool operator ==(Angle value1, Angle value2) => value1.Equals(value2);

    public static bool operator !=(Angle value1, Angle value2) => !value1.Equals(value2);

    /// <summary>
    /// Performance an equality check against another <see cref="Angle"/> object.
    /// The two objects are equal if both have matching <see cref="Degrees"/>, <see cref="Cos"/>, <see cref="Sin"/>, and <see cref="Direction"/> values.
    /// </summary>
    /// <param name="other">The <see cref="Angle"/> being compared against.</param>
    /// <returns>True if all checks are equal.</returns>
    public bool Equals(Angle other)
    {
        return this.degrees.Equals(other.degrees) && this.cos.Equals(other.cos) && this.sin.Equals(other.sin) && this.Direction.Equals(other.Direction);
    }

    /// <summary>
    /// Performance an equality check against another object.
    /// The two objects are equal if both are <see cref="Angle"/> types, have matching
    /// <see cref="Degrees"/>, <see cref="Cos"/>, <see cref="Sin"/>, and <see cref="Direction"/> values.
    /// </summary>
    /// <param name="obj">The object being compared against.</param>
    /// <returns>True if the other object is an angle and all checks are equal.</returns>
    public override bool Equals(object? obj)
    {
        return obj is Angle other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.degrees, this.cos, this.sin, this.Direction);
    }
}