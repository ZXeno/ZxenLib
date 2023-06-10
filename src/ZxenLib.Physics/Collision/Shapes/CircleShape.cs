namespace ZxenLib.Physics.Collision.Shapes;

using System;
using Microsoft.Xna.Framework;
using Collider;
using Common;

/// <summary>
/// A solid circle shape
/// </summary>
public class CircleShape : Shape
{
    /// Position
    public Vector2 Position;

    public new float Radius
    {
        get => base.Radius;
        set => base.Radius = value;
    }

    public CircleShape()
    {
        this.ShapeType = ShapeType.Circle;
        this.Radius = 0.0f;
        this.Position.SetZero();
    }

    /// Implement b2Shape.
    public override Shape Clone()
    {
        CircleShape? clone = new CircleShape {Position = this.Position, Radius = this.Radius};
        return clone;
    }

    /// @see b2Shape::GetChildCount
    public override int GetChildCount()
    {
        return 1;
    }

    /// Implement b2Shape.
    public override bool TestPoint(in Transform transform, in Vector2 p)
    {
        Vector2 center = transform.Position + MathUtils.Mul(transform.Rotation, this.Position);
        Vector2 d = p - center;
        return Vector2.Dot(d, d) <= this.Radius * this.Radius;
    }

    /// <summary>
    /// Implement b2Shape.
    /// @note because the circle is solid, rays that start inside do not hit because the normal is
    /// not defined.
    /// </summary>
    /// <param name="output"></param>
    /// <param name="input"></param>
    /// <param name="transform"></param>
    /// <param name="childIndex"></param>
    /// <returns></returns>
    public override bool RayCast(
        out RayCastOutput output,
        in RayCastInput input,
        in Transform transform,
        int childIndex)
    {
        output = default;
        Vector2 position = transform.Position + MathUtils.Mul(transform.Rotation, this.Position);
        Vector2 s = input.P1 - position;
        float b = Vector2.Dot(s, s) - this.Radius * this.Radius;

        // Solve quadratic equation.
        Vector2 r = input.P2 - input.P1;
        float c = Vector2.Dot(s, r);
        float rr = Vector2.Dot(r, r);
        float sigma = c * c - rr * b;

        // Check for negative discriminant and short segment.
        if (sigma < 0.0f || rr < Settings.Epsilon)
        {
            return false;
        }

        // Find the point of intersection of the line with the circle.
        float a = -(c + (float) Math.Sqrt(sigma));

        // Is the intersection point on the segment?
        if (0.0f <= a && a <= input.MaxFraction * rr)
        {
            a /= rr;
            output = new RayCastOutput {Fraction = a, Normal = s + a * r};
            output.Normal.Normalize();
            return true;
        }

        return false;
    }

    /// @see b2Shape::ComputeAABB
    public override void ComputeAABB(
        out AABB aabb,
        in Transform transform,
        int
            childIndex)
    {
        Vector2 p = transform.Position + MathUtils.Mul(transform.Rotation, this.Position);
        aabb = new AABB();
        aabb.LowerBound.Set(p.X - this.Radius, p.Y - this.Radius);
        aabb.UpperBound.Set(p.X + this.Radius, p.Y + this.Radius);
    }

    /// @see b2Shape::ComputeMass
    public override void ComputeMass(out MassData massData, float density)
    {
        massData = new MassData {Mass = density * Settings.Pi * this.Radius * this.Radius, Center = this.Position};

        // inertia about the local origin
        massData.RotationInertia = massData.Mass * (0.5f * this.Radius * this.Radius + Vector2.Dot(this.Position, this.Position));
    }
}