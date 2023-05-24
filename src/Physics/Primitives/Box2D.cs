namespace ZxenLib.Physics.Primitives;

using System;
using Extensions;
using Microsoft.Xna.Framework;

public class Box2D
{
    private Vector2 center;
    private Vector2 halfSize;
    private Vector2 size;
    private Vector2 position;
    private float rotation;

    public Box2D()
    {
        this.Position = new Vector2();
        this.Size = new Vector2(1f);
    }

    public Box2D(Vector2 min, Vector2 max)
    {
        this.Size = max.Clone() - min;
        this.Position = (max.Clone() + min) / 2f;
    }

    public Box2D(Vector2 size, Vector2 position, float rotation)
    {
        this.Size = size;
        this.Position = position;
        this.Rotation = rotation;
    }

    public Vector2 HalfSize => this.halfSize;

    public Vector2 Size
    {
        get => this.size;
        set
        {
            this.size = value;
            this.halfSize = new Vector2(
                this.size.X / 2f,
                this.size.Y / 2f);
        }
    }

    public Vector2 Position
    {
        get => this.position;
        set => this.position = value;
    }

    public float Rotation
    {
        get => this.rotation;
        set => this.rotation = value;
    }

    public Vector2 GetMin()
    {
        return this.position - this.halfSize;
    }

    public Vector2 GetMax()
    {
        return this.position + this.halfSize;
    }

    public Span<Vector2> GetVertices()
    {
        Vector2 min = this.GetMin();
        Vector2 max = this.GetMax();

        Span<Vector2> vertices = new Span<Vector2>(new[]
        {
            new Vector2(min.X, min.Y),
            new Vector2(min.X, max.Y),
            new Vector2(max.X, min.Y),
            new Vector2(max.X, max.Y),
        });

        for (int x = 0; x < vertices.Length; x++)
        {
            vertices[x].Rotate(this.position, this.rotation);
        }

        return vertices;
    }

    public bool Contains(Point point)
    {
        return Contains(point.ToVector2());
    }

    public bool Contains(Vector2 point)
    {
        Vector2 localPoint = new Vector2(point.X, point.Y);
        localPoint.Rotate(this.position, this.rotation);

        Vector2 min = this.GetMin();
        Vector2 max = this.GetMax();
        return localPoint.X <= max.X && min.X <= localPoint.X && localPoint.Y <= max.Y && min.Y <= localPoint.Y;
    }
}