namespace ZxenLib.Physics.Interfaces;

using Components;
using Microsoft.Xna.Framework;

public interface ICollider2D
{
    /// <summary>
    /// The position of this shape.
    /// </summary>
    Vector2 Position { get; }

    /// <summary>
    /// The <see cref="Rigidbody2D"/> containing the shape.
    /// </summary>
    Rigidbody2D Rigidbody { get; set; }

    /// <summary>
    /// Checks if a <see cref="Point"/> is contained inside the shape.
    /// </summary>
    /// <param name="point"><see cref="Point"/> coordinates being checked.</param>
    /// <returns>True if the point is inside the bounds of the shape.</returns>
    bool Contains(Point point);

    /// <summary>
    /// Checks if a <see cref="Vector2"/> is contained inside the shape.
    /// </summary>
    /// <param name="point"><see cref="Vector2"/> coordinates being checked.</param>
    /// <returns>True if the point is inside the bounds of the shape.</returns>
    bool Contains(Vector2 point);

    /// <summary>
    /// Checks if the coordinates are contained inside the shape.
    /// </summary>
    /// <param name="x">The x coordinate being checked.</param>
    /// <param name="y">The y coordinate being checked.</param>
    /// <returns>True if both coordinates are inside the bounds of the shape.</returns>
    bool Contains(float x, float y);

    /// <summary>
    /// Checks if the coordinates are contained inside the shape.
    /// </summary>
    /// <param name="x">The x coordinate being checked.</param>
    /// <param name="y">The y coordinate being checked.</param>
    /// <returns>True if both coordinates are inside the bounds of the shape.</returns>
    bool Contains(double x, double y);
}