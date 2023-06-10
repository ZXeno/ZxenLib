namespace ZxenLib.Physics.Primitives;

public static class Box2DExtensions
{
    public static AABB ToAabb(this Box2D box)
    {
        return new AABB(box.Position, box.Size.X, box.Size.Y);
    }
}