namespace ZxenLib.Physics.Interfaces;

using Microsoft.Xna.Framework;

public interface IContains2D
{
    bool Contains(Point point);

    bool Contains(Vector2 point);

    bool Contains(float x, float y);

    bool Contains(double x, double y);
}