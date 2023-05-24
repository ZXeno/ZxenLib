namespace ZxenLib.Physics;

using System;
using Extensions;
using Microsoft.Xna.Framework;
using Primitives;

public class IntersectionDetector2D
{
    public static bool PointOnLine(Vector2 point, Line2D line)
    {
        float dy = line.End.Y - line.Start.Y;
        float dx = line.End.X - line.Start.X;

        if (dx == 0f)
        {
            return ZxMath.Compare(point.X, line.Start.X);
        }

        float m = dy / dx;
        float b = line.End.Y - (m * line.End.X);

        return point.Y.Compare(m * point.X + b);
    }

    public static bool CircleContains(Vector2 point, Circle circle)
    {
        return circle.Contains(point);
    }

    public static bool AabbContains(Vector2 point, AABB box)
    {
        return box.Contains(point);
    }

    public static bool Box2dContains(Vector2 point, Box2D box)
    {
        return box.Contains(point);
    }

    public static bool LineIntersectsCircle(Line2D line, Circle circle)
    {
        if (circle.Contains(line.End) || circle.Contains(line.Start))
        {
            return true;
        }

        Vector2 ab = line.End.Clone() - line.Start;
        Vector2 circleCenter = circle.GetPositionVector();
        Vector2 centerToLineStart = circleCenter.Clone() - line.Start;
        float t = Vector2.Dot(centerToLineStart, ab) / Vector2.Dot(ab, ab);

        if (t < 0f || t > 1.0f)
        {
            return false;
        }

        Vector2 closestPoint = line.Start.Clone() + ab.Clone() * t;

        return circle.Contains(closestPoint);
    }

    public static bool LineIntersectsAabb(Line2D line, AABB box)
    {
        if (box.Contains(line.Start) || box.Contains(line.End))
        {
            return true;
        }

        Vector2 unitVector = line.End.Clone() - line.Start;
        unitVector.Normalize();
        unitVector.X = unitVector.X != 0 ? 1.0f / unitVector.X : float.MinValue;
        unitVector.Y = unitVector.Y != 0 ? 1.0f / unitVector.Y : float.MinValue;

        Vector2 min = box.GetMin();
        min = (min - line.Start.Clone()) * unitVector;
        Vector2 max = box.GetMax();
        max = (max - line.Start.Clone()) * unitVector;

        float tmin = Math.Max(Math.Min(min.X, max.X), Math.Min(min.Y, max.Y));
        float tmax = Math.Min(Math.Max(min.X, max.X), Math.Max(min.Y, max.Y));

        if (tmax < 0 || tmin > tmax)
        {
            return false;
        }

        float t = (tmin < 0f) ? tmax : tmin;
        return t > 0f && t * t < line.LengthSquared();
    }

    public static bool LineIntersectsBox2D(Line2D line, Box2D box)
    {
        float theta = -box.Rotation;
        Vector2 center = box.Position.Clone();
        Vector2 localStart = line.Start.Clone();
        Vector2 localEnd = line.End.Clone();

        localStart.Rotate(center, theta);
        localEnd.Rotate(center, theta);

        Line2D localLine = new Line2D(localStart, localEnd);
        AABB aabb = new AABB(box.GetMin(), box.GetMax());

        return LineIntersectsAabb(localLine, aabb);
    }
}