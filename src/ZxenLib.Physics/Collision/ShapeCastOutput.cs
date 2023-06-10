namespace ZxenLib.Physics.Collision;

using Microsoft.Xna.Framework;

/// Output results for b2ShapeCast
public struct ShapeCastOutput
{
    public Vector2 Point;

    public Vector2 Normal;

    public float Lambda;

    public int Iterations;
}