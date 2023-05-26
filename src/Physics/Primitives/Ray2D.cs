namespace ZxenLib.Physics.Primitives;

using Microsoft.Xna.Framework;

public class Ray2D
{
    private Vector2 origin;
    private Vector2 direction;

    public Ray2D(Vector2 origin, Vector2 direction)
    {
        this.origin = origin;
        this.direction = direction;
        this.direction.Normalize();
    }

    public Vector2 Origin => this.origin;

    public Vector2 Direction => this.direction;
}