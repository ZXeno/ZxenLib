namespace ZxenLib.Physics.Primitives;

using System;
using Extensions;
using Interfaces;
using Microsoft.Xna.Framework;

public struct Circle : IContains2D
{
    private float x;
    private float y;
    private float radius;
    private float rSqr;

    public Circle()
    {
        this.x = 0;
        this.y = 0;
        this.radius = 1f;
        this.rSqr = this.radius * this.radius;
    }

    public Circle(float x, float y, float radius)
    {
        this.x = x;
        this.y = y;
        this.radius = Math.Clamp(radius, 0, float.MaxValue);
        this.rSqr = radius * radius;
    }

    public Circle(Vector2 position, float radius)
    {
        this.x = position.X;
        this.y = position.Y;
        this.radius = radius;
        this.rSqr = radius.Sqr();
    }

    public float X
    {
        get => this.x;
        set => this.x = value;
    }

    public float Y
    {
        get => this.y;
        set => this.y = value;
    }

    public float Radius
    {
        get => this.radius;
        set
        {
            this.radius = value;
            this.rSqr = this.radius.Sqr();
        }
    }

    public float RSqr => this.rSqr;

    public bool Contains(Point point)
    {
        return this.Contains((double)point.X, (double)point.Y);
    }

    public bool Contains(Vector2 point)
    {
        return this.Contains((double)point.X, (double)point.Y);
    }

    public bool Contains(float px, float py)
    {
        return this.Contains((double)px, (double)py);
    }

    public bool Contains(double px, double py)
    {
        double xDistance = Math.Abs(this.x - px);
        double yDistance = Math.Abs(this.y - py);

        return xDistance <= this.radius && yDistance <= this.radius;
    }

    public Vector2 GetPositionVector()
    {
        return new Vector2((float)this.x, (float)this.y);
    }
}