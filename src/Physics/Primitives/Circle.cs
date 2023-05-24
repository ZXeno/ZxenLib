namespace ZxenLib.Physics.Primitives;

using System;
using Microsoft.Xna.Framework;

public struct Circle
{
    public double X;
    public double Y;
    public double Radius;

    public Circle()
    {
        this.X = 0;
        this.Y = 0;
        this.Radius = 1f;
    }

    public Circle(double x, double y, double radius)
    {
        this.X = x;
        this.Y = y;
        this.Radius = radius;
    }

    public bool Contains(Point point)
    {
        return this.Contains((double)point.X, (double)point.Y);
    }

    public bool Contains(Vector2 point)
    {
        return this.Contains((double)point.X, (double)point.Y);
    }

    public bool Contains(float x, float y)
    {
        return this.Contains((double)x, (double)y);
    }

    public bool Contains(double x, double y)
    {
        double xDistance = Math.Abs(this.X - x);
        double yDistance = Math.Abs(this.Y - y);

        return xDistance <= this.Radius && yDistance <= this.Radius;
    }

    public Vector2 GetPositionVector()
    {
        return new Vector2((float)this.X, (float)this.Y);
    }
}