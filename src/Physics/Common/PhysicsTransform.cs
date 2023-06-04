namespace ZxenLib.Physics.Common;

using System;
using Components;
using Entities.Components;
using Microsoft.Xna.Framework;

public class PhysicsTransform
{
    public static PhysicsTransform Identity = new();

    private Vector2 position;
    private Rotation rotation;

    /// <summary>
    /// Creates a new instance of the <see cref="PhysicsTransform"/> class.
    /// </summary>
    public PhysicsTransform()
    {
        this.position = new();
        this.rotation = new();
    }

    /// <summary>
    /// Creates a new instance of the <see cref="PhysicsTransform"/> class.
    /// </summary>
    /// <param name="position">The starting position of the transform.</param>
    public PhysicsTransform(Vector2 position)
    {
        this.position = position;
        this.rotation = new();
    }

    /// <summary>
    /// Creates a new instance of the <see cref="PhysicsTransform"/> class.
    /// </summary>
    /// <param name="position">The starting position of the transform.</param>
    /// <param name="rotation">The starting rotation of the transform.</param>
    public PhysicsTransform(Vector2 position, Rotation rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="PhysicsTransform"/> class.
    /// </summary>
    /// <param name="position">The starting position of the transform.</param>
    /// <param name="angle">The starting rotation angle of the transform in radians.</param>
    public PhysicsTransform(Vector2 position, float angle)
    {
        this.position = position;
        this.rotation = new(angle);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="PhysicsTransform"/> class.
    /// </summary>
    /// <param name="rigidbody">The rigidbody to pull new transform data from. Will use the <see cref="Transform"/> and not the <see cref="Rigidbody2D"/>'s <see cref="PhysicsTransform"/> property!</param>
    public PhysicsTransform(Rigidbody2D rigidbody)
    {
        this.position = rigidbody.ParentTransform.Position;
        this.rotation = new(rigidbody.ParentTransform.Angle.Radians);
    }

    /// <summary>
    /// Gets the <see cref="Vector2"/> position of this <see cref="PhysicsTransform"/>.
    /// </summary>
    public Vector2 Position => this.position;

    /// <summary>
    /// Gets or sets the rotation of this <see cref="PhysicsTransform"/>.
    /// </summary>
    public Rotation Rotation => this.rotation;

    /// <summary>
    /// Resets this transform to it's Identity state.
    /// </summary>
    public void SetIdentity()
    {
        this.position = Vector2.Zero;
        this.rotation.SetIdentity();
    }

    /// Set this based on the position and angle.
    public void Set(Vector2 pos, float angle)
    {
        this.position = pos;
        this.rotation.Set(angle);
    }

    /// <summary>
    /// Applies a rotation and translation to the input vector.
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vector2 Mul(PhysicsTransform transform, Vector2 vector)
    {
        return new Vector2(
            transform.Rotation.Cos * vector.X - transform.Rotation.Sin * vector.Y + transform.Position.X,
            transform.Rotation.Sin * vector.X + transform.Rotation.Cos * vector.Y + transform.Position.Y);
    }

    /// <summary>
    /// Translates the input vector back to the origin and applies the inverse rotation. (the reverse of <see cref="Mul"/>).
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vector2 MulT(PhysicsTransform transform, Vector2 vector)
    {
        float px = vector.X - transform.Position.X;
        float py = vector.Y - transform.Position.Y;
        return new Vector2(
            transform.Rotation.Cos * px + transform.Rotation.Sin * py,
            -transform.Rotation.Sin * px + transform.Rotation.Cos * py);
    }

    public override string ToString()
    {
        return $"({this.position.X},{this.position.Y}), Cos:{this.rotation.Cos}, Sin:{this.rotation.Sin})";
    }
}