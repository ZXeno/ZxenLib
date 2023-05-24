// ReSharper disable InconsistentNaming
namespace ZxenLib.Physics.Primitives;

using Extensions;
using Microsoft.Xna.Framework;

/// <summary>
/// Axis-Aligned Bounding Box
/// </summary>
public class AABB
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
        this.Size = max.Clone() - min;
        this.Position = (max.Clone() + min) / 2f;
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

    public bool Contains(Vector2 point)
    {
        return this.Contains((double)point.X, point.Y);
    }

    public bool Contains(Point point)
    {
        return this.Contains((double)point.X, point.Y);
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
}