namespace ZxenLib.Physics.Primitives;

using Extensions;
using Microsoft.Xna.Framework;

public class Line2D
{
    public Line2D(Vector2 start, Vector2 end)
    {
        this.Start = start;
        this.End = end;
    }

    public Line2D(Vector2 start, Vector2 end, Color color)
    {
        this.Start = start;
        this.End = end;
    }

    public Vector2 Start { get; set; }

    public Vector2 End { get; set; }

    public float LengthSquared()
    {
        return Vector2.DistanceSquared(this.Start, this.End);
    }
}