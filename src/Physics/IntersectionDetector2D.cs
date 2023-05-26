namespace ZxenLib.Physics;

using System;
using Extensions;
using Interfaces;
using Microsoft.Xna.Framework;
using Primitives;

public class IntersectionDetector2D
{
    private const float Epsilon = .000001f;

    // Contains
    public static bool PointOnLine(Vector2 point, Line2D line)
    {
        float dy = line.End.Y - line.Start.Y;
        float dx = line.End.X - line.Start.X;

        if (dx == 0f)
        {
            return ZxMath.Compare(point.X, line.Start.X, Epsilon);
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

    // Line intersects
    public static bool LineIntersectsCircle(Line2D line, Circle circle)
    {
        if (circle.Contains(line.End) || circle.Contains(line.Start))
        {
            return true;
        }

        Vector2 ab = line.End - line.Start;
        Vector2 circleCenter = circle.GetPositionVector();
        Vector2 centerToLineStart = circleCenter - line.Start;
        float t = Vector2.Dot(centerToLineStart, ab) / Vector2.Dot(ab, ab);

        if (t < 0f || t > 1.0f)
        {
            return false;
        }

        Vector2 closestPoint = line.Start + ab * t;

        return circle.Contains(closestPoint);
    }

    public static bool LineIntersectsAabb(Line2D line, AABB box)
    {
        if (box.Contains(line.Start) || box.Contains(line.End))
        {
            return true;
        }

        Vector2 unitVector = line.End - line.Start;
        unitVector.Normalize();
        unitVector.X = unitVector.X != 0 ? 1.0f / unitVector.X : float.MinValue;
        unitVector.Y = unitVector.Y != 0 ? 1.0f / unitVector.Y : float.MinValue;

        Vector2 min = box.GetMin();
        min = (min - line.Start) * unitVector;
        Vector2 max = box.GetMax();
        max = (max - line.Start) * unitVector;

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
        AABB aabb = new AABB(box.GetLocalMin(), box.GetLocalMax());

        return LineIntersectsAabb(localLine, aabb);
    }

    // Raycasts
    public static bool Raycast(Circle circle, Ray2D ray, RaycastResult? result)
    {
        result?.Reset();

        Vector2 originToCircle = circle.GetPositionVector().Clone() - ray.Origin;
        float radiusSquared = (float)circle.RSqr;
        float originToCircleLengthSqr = originToCircle.LengthSquared();

        float a = Vector2.Dot(originToCircle, ray.Direction);
        float bSqr = originToCircleLengthSqr - (a * a);

        if (radiusSquared - bSqr < 0)
        {
            return false;
        }

        float f = (float)Math.Sqrt(radiusSquared - bSqr);
        float t = 0;
        if (originToCircleLengthSqr < radiusSquared)
        {
            t = a + f;
        }
        else
        {
            t = a - f;
        }

        if (t < 0)
        {
            return false;
        }

        if (result != null)
        {
            Vector2 point = ray.Origin.Clone() + ray.Direction.Clone() * t;
            Vector2 normal = point.Clone() - circle.GetPositionVector();
            normal.Normalize();

            result.Init(point, normal, t, true);
        }

        return true;
    }

    public static bool Raycast(AABB box, Ray2D ray, RaycastResult? result)
    {
        result?.Reset();

        Vector2 unitVector = ray.Direction.Clone();
        unitVector.Normalize();
        unitVector.X = unitVector.X != 0 ? 1.0f / unitVector.X : float.MinValue;
        unitVector.Y = unitVector.Y != 0 ? 1.0f / unitVector.Y : float.MinValue;

        Vector2 min = box.GetMin();
        min = (min - ray.Origin) * unitVector;
        Vector2 max = box.GetMax();
        max = (max - ray.Origin) * unitVector;

        float tmin = Math.Max(Math.Min(min.X, max.X), Math.Min(min.Y, max.Y));
        float tmax = Math.Min(Math.Max(min.X, max.X), Math.Max(min.Y, max.Y));

        if (tmax < 0 || tmin > tmax)
        {
            return false;
        }

        float t = (tmin < 0f) ? tmax : tmin;
        bool hit = t > 0f; // && t * t < line.LengthSquared();
        if (!hit)
        {
            return false;
        }

        if (result != null)
        {
            Vector2 point = ray.Origin + ray.Direction * t;
            Vector2 normal = ray.Origin - point;
            normal.Normalize();

            result.Init(point, normal, t, hit);
        }

        return true;
    }

    public static bool Raycast(Box2D box, Ray2D ray, RaycastResult? result)
    {
        result?.Reset();

        float theta = -box.Rotation;
        Vector2 center = box.Position.Clone();
        Vector2 localRayOrigin = ray.Origin.Clone();
        Vector2 localRayDirection = ray.Direction.Clone();
        localRayOrigin.Rotate(center, theta);
        localRayDirection.Rotate(center, theta);

        AABB aabb = box.ToAabb();
        bool hit = Raycast(aabb, new Ray2D(localRayOrigin, localRayDirection), result);
        if (!hit)
        {
            return false;
        }

        result?.Point.Rotate(center, box.Rotation);
        result?.Normal.Rotate(center, box.Rotation);

        return true;
    }

    // Circle <-> Shape intersection
    public static bool CircleVsCircle(Circle circle1, Circle circle2)
    {
        float distanceSqr = Vector2.DistanceSquared(circle1.GetPositionVector(), circle2.GetPositionVector());
        float radiiSum = (float)circle1.Radius + (float)circle2.Radius;
        return distanceSqr <= radiiSum.Sqr();
    }

    public static bool CircleVsAabb(Circle circle, AABB box)
    {
        return CalculateCircleToBoxes(circle.GetPositionVector(), circle.Radius, box.GetMin(), box.GetMax());
    }

    public static bool CircleVsBox2D(Circle circle, Box2D box)
    {
        Vector2 min = Vector2.Zero;
        Vector2 max = box.HalfSize * 2f;

        Vector2 r = circle.GetPositionVector() - box.Position;
        r.Rotate(Vector2.Zero, -box.Rotation);
        Vector2 localCirclePos = r + box.HalfSize;

        return CalculateCircleToBoxes(localCirclePos, circle.Radius, min, max);
    }

    private static bool CalculateCircleToBoxes(Vector2 circlePos, float radius, Vector2 boxMin, Vector2 boxMax)
    {
        Vector2 closestPointToCircle = circlePos.Clone();
        if (closestPointToCircle.X < boxMin.X)
        {
            closestPointToCircle.X = boxMin.X;
        }
        else if (closestPointToCircle.X > boxMax.X)
        {
            closestPointToCircle.X = boxMax.X;
        }

        if (closestPointToCircle.Y < boxMin.Y)
        {
            closestPointToCircle.Y = boxMin.Y;
        }
        else if (closestPointToCircle.Y > boxMax.Y)
        {
            closestPointToCircle.Y = boxMax.Y;
        }

        Vector2 circleToBox = circlePos - closestPointToCircle;
        return circleToBox.LengthSquared() <= radius.Sqr();
    }

    // IVertexes2D <-> IVertexes2D shape intersections
    public static bool VertexShapeVsVertexShape(IVertexes2D shape1, IVertexes2D shape2)
    {
        return PhysicsHelper.SatDetectOverlap(shape1.GetVertices(), shape2.GetVertices());
    }
}