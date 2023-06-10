namespace ZxenLib.Physics.Collision;

using Microsoft.Xna.Framework;

/// Output for b2Distance.
public struct DistanceOutput
{
    /// closest point on shapeA
    public Vector2 PointA;

    /// closest point on shapeB
    public Vector2 PointB;

    public float Distance;

    /// number of GJK iterations used
    public int Iterations;
}