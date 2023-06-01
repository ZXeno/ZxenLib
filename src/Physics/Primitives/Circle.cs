namespace ZxenLib.Physics.Primitives;

using System;
using Extensions;
using Interfaces;
using Microsoft.Xna.Framework;

/// <summary>
/// Struct representing a circle.
/// </summary>
public class Circle : IShape
{
    private Vector2 position;
    private float radius;
    private float rSqr;
    private Rigidbody2D? rigidBody;

    /// <summary>
    /// Creates a new <see cref="Circle"/> at X: 0, Y: 0, with a radius of 1.
    /// </summary>
    public Circle()
    {
        this.position = new Vector2();
        this.radius = 1f;
        this.rSqr = this.radius.Sqr();
    }

    /// <summary>
    /// Creates a new <see cref="Circle"/> with the specified values.
    /// </summary>
    /// <param name="x">The X coordinate of the circle.</param>
    /// <param name="y">The Y coordinate of the circle.</param>
    /// <param name="radius"></param>
    public Circle(float x, float y, float radius)
    {
        this.position = new Vector2(x, y);
        this.radius = Math.Clamp(radius, 0, float.MaxValue);
        this.rSqr = radius * radius;
    }

    /// <summary>
    /// Creates a new <see cref="Circle"/> with the specified values.
    /// </summary>
    /// <param name="position">The position of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    public Circle(Vector2 position, float radius)
    {
        this.position = position;
        this.radius = radius;
        this.rSqr = radius.Sqr();
    }

    /// <summary>
    /// The radius of the circle.
    /// </summary>
    public float Radius
    {
        get => this.radius;
        set
        {
            this.radius = value;
            this.rSqr = this.radius.Sqr();
        }
    }

    /// <summary>
    /// The Radius squared of the circle.
    /// </summary>
    public float RSqr => this.rSqr;

    public Vector2 Position
    {
        get => this.position;
        set => this.position = value;
    }

    public Vector2 WorldPosition => this.rigidBody?.WorldPosition ?? Vector2.Zero + this.Position;

    public Rigidbody2D? Rigidbody
    {
        get => this.rigidBody;
        set => this.rigidBody = value;
    }

    /// <summary>
    /// Checks if a <see cref="Point"/> is located inside the circle.
    /// </summary>
    /// <param name="point">The <see cref="Point"/> coordinates being checked.</param>
    /// <returns>True if the point.X and point.Y values are both located inside the circle.</returns>
    public bool Contains(Point point)
    {
        return this.Contains((double)point.X, (double)point.Y);
    }

    /// <summary>
    /// Check if a <see cref="Vector2"/> is located inside the circle.
    /// </summary>
    /// <param name="point">The <see cref="Vector2"/> coordinates being checked.</param>
    /// <returns>True if the point.X and point.Y values are both located inside the circle.</returns>
    public bool Contains(Vector2 point)
    {
        return this.Contains((double)point.X, (double)point.Y);
    }

    /// <summary>
    /// Checks if the floating point value coordinates are located inside the circle.
    /// </summary>
    /// <param name="px">The x axis coordinate being checked.</param>
    /// <param name="py">The y axis coordinate being checked.</param>
    /// <returns>True if the px and py values are both located inside the circle.</returns>
    public bool Contains(float px, float py)
    {
        return this.Contains((double)px, (double)py);
    }

    /// <summary>
    /// Checks if the <see cref="double"/> value coordinates are located inside the circle.
    /// </summary>
    /// <param name="px">The x axis coordinate being checked.</param>
    /// <param name="py">The y axis coordinate being checked.</param>
    /// <returns>True if the px and py values are both located inside the circle.</returns>
    public bool Contains(double px, double py)
    {
        double xDistance = Math.Abs(this.position.X - px);
        double yDistance = Math.Abs(this.position.Y - py);

        return xDistance <= this.radius && yDistance <= this.radius;
    }
}