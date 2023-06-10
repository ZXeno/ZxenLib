namespace ZxenLib.Physics.Common;

using System;
using Microsoft.Xna.Framework;

public struct Transform : IFormattable
{
    public Vector2 Position;

    public Rotation Rotation;

    /// Initialize using a position vector and a rotation.
    public Transform(in Vector2 position, in Rotation rotation)
    {
        this.Position = position;
        this.Rotation = rotation;
    }

    public Transform(in Vector2 position, float angle)
    {
        this.Position = position;
        this.Rotation = new Rotation(angle);
    }

    /// Set this to the identity transform.
    public void SetIdentity()
    {
        this.Position = Vector2.Zero;
        this.Rotation.SetIdentity();
    }

    /// Set this based on the position and angle.
    public void Set(in Vector2 position, float angle)
    {
        this.Position = position;
        this.Rotation.Set(angle);
    }

    /// <inheritdoc />
    public string ToString(string format, IFormatProvider formatProvider)
    {
        return this.ToString();
    }

    public new string ToString()
    {
        return $"({this.Position.X},{this.Position.Y}), Cos:{this.Rotation.Cos}, Sin:{this.Rotation.Sin})";
    }
}