// ReSharper disable InconsistentNaming
namespace ZxenLib.Physics.Primitives;

using System;
using System.Diagnostics;
using Interfaces;
using Microsoft.Xna.Framework;

/// <summary>
/// Axis-Aligned Bounding Box
/// </summary>
public class AABB : IPolygon2D, IContains2D
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

    public Vector2 Center { get; set; }

    /// <summary>
    /// For all other shapes, this should be set. But the AABB should always have a rotation of 0
    /// </summary>
    public float Rotation
    {
        get => 0f;
        set
        {
            Debug.Assert(value == 0, "Rotation of an AABB should always be 0.");
            return;
        }
    }

    public Vector2 HalfSize => this.halfSize;

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

    public Vector2 GetLocalMin()
    {
        return this.position - this.halfSize;
    }

    public Vector2 GetLocalMax()
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
        Vector2 min = this.GetLocalMin();
        Vector2 max = this.GetLocalMax();

        return x >= min.X && x <= max.X && y >= min.Y && y <= max.Y;
    }

    public Span<Vector2> GetVertices()
    {
        Vector2 min = this.GetLocalMin();
        Vector2 max = this.GetLocalMax();

        return new Span<Vector2>(new[]
        {
            new Vector2(min.X, min.Y),
            new Vector2(min.X, max.Y),
            new Vector2(max.X, min.Y),
            new Vector2(max.X, max.Y),
        });
    }
}