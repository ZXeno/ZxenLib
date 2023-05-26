// ReSharper disable InconsistentNaming
namespace ZxenLib.Physics.Primitives;

using System;
using Extensions;
using Interfaces;
using Microsoft.Xna.Framework;

/// <summary>
/// Axis-Aligned Bounding Box
/// </summary>
public class AABB : IVertexes2D, IContains2D
{
    private Vector2 halfSize;
    private Vector2 size;
    private Vector2 position;

    public AABB()
    {
        this.Position = new Vector2();
        this.Size = new Vector2(1f);
    }

    public AABB(Vector2 min, Vector2 max)
    {
        this.Size = max - min;
        this.Position = (max + min) / 2f;
    }

    public AABB(Vector2 position, float size)
    {
        this.Size = new(Math.Abs(size));
        this.Position = position;
    }

    public AABB(Vector2 position, float sizeX, float sizeY)
    {
        this.Size = new(Math.Abs(sizeX), Math.Abs(sizeY));
        this.Position = position;
    }

    public Vector2 Size
    {
        get => this.size;
        set
        {
            this.size = value;
            this.halfSize = this.size / 2f;
        }
    }

    public Vector2 Position
    {
        get => this.position;
        set => this.position = value;
    }

    public Vector2 GetMin()
    {
        return this.position - this.halfSize;
    }

    public Vector2 GetMax()
    {
        return this.position + this.halfSize;
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
        Vector2 min = this.GetMin();
        Vector2 max = this.GetMax();

        return x >= min.X && x <= max.X && y >= min.Y && y <= max.Y;
    }

    public Span<Vector2> GetVertices()
    {
        Vector2 min = this.GetMin();
        Vector2 max = this.GetMax();

        return new Span<Vector2>(new[]
        {
            new Vector2(min.X, min.Y),
            new Vector2(min.X, max.Y),
            new Vector2(max.X, min.Y),
            new Vector2(max.X, max.Y),
        });
    }
}