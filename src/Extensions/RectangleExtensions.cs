namespace ZxenLib.Extensions;

using System;
using Microsoft.Xna.Framework;

public static class RectangleExtensions
{
    public static Vector2 GetIntersectionDepth(this Rectangle rectA, Rectangle rectB)
    {
        // Calculate half sizes.
        float halfWidthA = rectA.Width / 2.0f;
        float halfHeightA = rectA.Height / 2.0f;
        float halfWidthB = rectB.Width / 2.0f;
        float halfHeightB = rectB.Height / 2.0f;

        // Calculate centers.
        Vector2 centerA = new Vector2(rectA.Left + halfWidthA, rectA.Top + halfHeightA);
        Vector2 centerB = new Vector2(rectB.Left + halfWidthB, rectB.Top + halfHeightB);

        // Calculate current and minimum-non-intersecting distances between centers.
        float distanceX = centerA.X - centerB.X;
        float distanceY = centerA.Y - centerB.Y;
        float minDistanceX = halfWidthA + halfWidthB;
        float minDistanceY = halfHeightA + halfHeightB;

        // If we are not intersecting at all, return (0, 0).
        if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
            return Vector2.Zero;

        // Calculate and return intersection depths.
        float depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
        float depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;
        return new Vector2(depthX, depthY);
    }

    /// <summary>
    /// Get the bottom-center position of the rectangle.
    /// </summary>
    /// <param name="rect"></param>
    /// <returns>Vector2 point representing bottom-center of the rectangle.</returns>
    public static Vector2 GetBottomCenter(this Rectangle rect)
    {
        return new Vector2(rect.X + rect.Width / 2.0f, rect.Bottom);
    }

    /// <summary>
    /// Get the left-center position of the rectangle.
    /// </summary>
    /// <param name="rect"></param>
    /// <returns>Vector2 point representing left-center of the rectangle.</returns>
    public static Vector2 GetLeftCenter(this Rectangle rect)
    {
        return new Vector2(rect.Left, rect.Y + rect.Height / 2.0f);
    }

    /// <summary>
    /// Get the right-center position of the rectangle.
    /// </summary>
    /// <param name="rect"></param>
    /// <returns>Vector2 point representing right-center of the rectangle.</returns>
    public static Vector2 GetRightCenter(this Rectangle rect)
    {
        return new Vector2(rect.Right, rect.Y + rect.Height/2.0f);
    }

    /// <summary>
    /// Get the top-center position of the rectangle.
    /// </summary>
    /// <param name="rect"></param>
    /// <returns>Vector2 point representing top-center of the rectangle.</returns>
    public static Vector2 GetTopCenter(this Rectangle rect)
    {
        return new Vector2(rect.X + rect.Width / 2.0f, rect.Top);
    }
}