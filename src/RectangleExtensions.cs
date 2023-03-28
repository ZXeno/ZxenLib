namespace ZxenLib;

using System;
using Microsoft.Xna.Framework;

/// <summary>
/// Extensions for the Monogame <see cref="Rectangle"/>.
/// </summary>
public static class RectangleExtensions
{
    /// <summary>
    /// Determines intersection depth of two <see cref="Rectangle"/>.
    /// </summary>
    /// <param name="boxA">The first rect.</param>
    /// <param name="boxB">The second rect.</param>
    /// <returns>Returns a <see cref="Vector2"/> indicating depth and direction.</returns>
    public static Vector2 GetIntersectionDepth(this Rectangle boxA, Rectangle boxB)
    {
        // Calculate half sizes.
        float halfWidthA = boxA.Width / 2.0f;
        float halfHeightA = boxA.Height / 2.0f;
        float halfWidthB = boxB.Width / 2.0f;
        float halfHeightB = boxB.Height / 2.0f;

        // Calculate centers.
        Vector2 centerA = new Vector2(boxA.Left + halfWidthA, boxA.Top + halfHeightA);
        Vector2 centerB = new Vector2(boxA.Left + halfWidthB, boxB.Top + halfHeightB);

        // Calculate current and minimum-non-intersecting distances between centers.
        float distanceX = centerA.X - centerB.X;
        float distanceY = centerA.Y - centerB.Y;
        float minDistanceX = halfWidthA + halfWidthB;
        float minDistanceY = halfHeightA + halfHeightB;

        // If we are not intersecting at all, return (0, 0).
        if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
        {
            return Vector2.Zero;
        }

        // Calculate and return intersection depths.
        float depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
        float depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;
        return new Vector2(depthX, depthY);
    }

    /// <summary>
    /// Gets the verticies of a rectangle as an array.
    /// </summary>
    /// <param name="rect">The rect to get verticies from.</param>
    /// <returns><see cref="Vector2[]"/> of the rectangle verticies.</returns>
    public static Vector2[] GetVerticies(this Rectangle rect)
    {
        Vector2[] dots = new Vector2[4]; // 4 corners
        dots[0] = new Vector2(rect.X, rect.Y);
        dots[1] = new Vector2(rect.X + rect.Width, rect.Y);
        dots[2] = new Vector2(rect.X + rect.Width, rect.Y + rect.Height);
        dots[3] = new Vector2(rect.X, rect.Y + rect.Height);

        return dots;
    }
}