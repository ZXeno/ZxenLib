namespace ZxenLib.Physics.Collision.Shapes;

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Collider;
using Common;

/// <summary>
/// A solid convex polygon. It is assumed that the interior of the polygon is to
/// the left of each edge.
/// Polygons have a maximum number of vertices equal to b2_maxPolygonVertices.
/// In most cases you should not need many vertices for a convex polygon.
/// </summary>
public class PolygonShape : Shape
{
    public const int MaxPolygonVertices = Settings.MaxPolygonVertices;

    public readonly Vector2[] Normals = new Vector2[MaxPolygonVertices];

    public readonly Vector2[] Vertices = new Vector2[MaxPolygonVertices];

    public Vector2 Centroid;

    public int Count;

    public PolygonShape()
    {
        this.ShapeType = ShapeType.Polygon;
        this.Radius = Settings.PolygonRadius;
    }

    /// Implement b2Shape.
    public override Shape Clone()
    {
        PolygonShape? clone = new PolygonShape {Centroid = this.Centroid, Count = this.Count};
        Array.Copy(this.Vertices, clone.Vertices, this.Vertices.Length);
        Array.Copy(this.Normals, clone.Normals, this.Normals.Length);
        return clone;
    }

    /// @see b2Shape::GetChildCount
    public override int GetChildCount()
    {
        return 1;
    }

    /// Create a convex hull from the given array of local points.
    /// The count must be in the range [3, b2_maxPolygonVertices].
    /// @warning the points may be re-ordered, even if they form a convex polygon
    /// @warning collinear points are handled but not removed. Collinear points
    /// may lead to poor stacking behavior.
    public void Set(Vector2[] vertices, int count = -1)
    {
        if (count == -1)
        {
            count = vertices.Length;
        }

        Debug.Assert(3 <= count && count <= MaxPolygonVertices);

        // 顶点数小于3,视为盒子
        if (count < 3)
        {
            this.SetAsBox(1.0f, 1.0f);
            return;
        }

        // 顶点数最大为 MaxPolygonVertices
        int n = Math.Min(count, MaxPolygonVertices);

        // Perform welding and copy vertices into local buffer.
        Span<Vector2> ps = stackalloc Vector2[MaxPolygonVertices];

        int tempCount = 0;
        for (int i = 0; i < n; ++i)
        {
            Vector2 v = vertices[i];

            bool unique = true;
            for (int j = 0; j < tempCount; ++j)
            {
                if (Vector2.DistanceSquared(v, ps[j])
                    < 0.5f * Settings.LinearSlop * (0.5f * Settings.LinearSlop))
                {
                    unique = false;
                    break;
                }
            }

            if (unique)
            {
                ps[tempCount] = v;
                tempCount++;
            }
        }

        n = tempCount;
        if (n < 3)
        {
            // Polygon is degenerate.
            throw new InvalidOperationException("Invalid polygon shape");
        }

        // Create the convex hull using the Gift wrapping algorithm
        // http://en.wikipedia.org/wiki/Gift_wrapping_algorithm

        // Find the right most point on the hull
        int i0 = 0;
        float x0 = ps[0].X;
        for (int i = 1; i < n; ++i)
        {
            float x = ps[i].X;
            if (x > x0 || x.Equals(x0) && ps[i].Y < ps[i0].Y)
            {
                i0 = i;
                x0 = x;
            }
        }

        Span<int> hull = stackalloc int[MaxPolygonVertices];
        int m = 0;
        int ih = i0;

        for (;;)
        {
            Debug.Assert(m < MaxPolygonVertices);
            hull[m] = ih;

            int ie = 0;
            for (int j = 1; j < n; ++j)
            {
                if (ie == ih)
                {
                    ie = j;
                    continue;
                }

                Vector2 r = ps[ie] - ps[hull[m]];
                Vector2 v = ps[j] - ps[hull[m]];
                float c = MathUtils.Cross(r, v);
                if (c < 0.0f)
                {
                    ie = j;
                }

                // Collinearity check
                if (c.Equals(0.0f) && v.LengthSquared() > r.LengthSquared())
                {
                    ie = j;
                }
            }

            ++m;
            ih = ie;

            if (ie == i0)
            {
                break;
            }
        }

        if (m < 3)
        {
            // Polygon is degenerate.
            throw new InvalidOperationException("Invalid polygon shape");
        }

        this.Count = m;

        // Copy vertices.
        for (int i = 0; i < m; ++i)
        {
            this.Vertices[i] = ps[hull[i]];
        }

        // Compute normals. Ensure the edges have non-zero length.
        for (int i = 0; i < m; ++i)
        {
            int i1 = i;
            int i2 = i + 1 < m ? i + 1 : 0;
            Vector2 edge = this.Vertices[i2] - this.Vertices[i1];
            Debug.Assert(edge.LengthSquared() > Settings.Epsilon * Settings.Epsilon);
            this.Normals[i] = MathUtils.Cross(edge, 1.0f);
            this.Normals[i].Normalize();
        }

        // Compute the polygon centroid.
        this.Centroid = ComputeCentroid(this.Vertices, m);
    }

    /// Build vertices to represent an axis-aligned box centered on the local origin.
    /// @param hx the half-width.
    /// @param hy the half-height.
    public void SetAsBox(float hx, float hy)
    {
        this.Count = 4;
        this.Vertices[0].Set(-hx, -hy);
        this.Vertices[1].Set(hx, -hy);
        this.Vertices[2].Set(hx, hy);
        this.Vertices[3].Set(-hx, hy);
        this.Normals[0].Set(0.0f, -1.0f);
        this.Normals[1].Set(1.0f, 0.0f);
        this.Normals[2].Set(0.0f, 1.0f);
        this.Normals[3].Set(-1.0f, 0.0f);
        this.Centroid.SetZero();
    }

    /// Build vertices to represent an oriented box.
    /// @param hx the half-width.
    /// @param hy the half-height.
    /// @param center the center of the box in local coordinates.
    /// @param angle the rotation of the box in local coordinates.
    public void SetAsBox(float hx, float hy, in Vector2 center, float angle)
    {
        this.SetAsBox(hx, hy);
        this.Centroid = center;
        Transform transform = new Transform(in center, angle);

        // Transform vertices and normals.
        for (int i = 0; i < this.Count; ++i)
        {
            this.Vertices[i] = MathUtils.Mul(transform, this.Vertices[i]);
            this.Normals[i] = MathUtils.Mul(transform.Rotation, this.Normals[i]);
        }
    }

    /// @see b2Shape::TestPoint
    public override bool TestPoint(in Transform transform, in Vector2 p)
    {
        Vector2 pLocal = MathUtils.MulT(transform.Rotation, p - transform.Position);

        for (int i = 0; i < this.Count; ++i)
        {
            float dot = Vector2.Dot(this.Normals[i], pLocal - this.Vertices[i]);
            if (dot > 0.0f)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Implement b2Shape.
    /// @note because the polygon is solid, rays that start inside do not hit because the normal is
    /// not defined.
    /// </summary>
    /// <param name="output"></param>
    /// <param name="input"></param>
    /// <param name="transform"></param>
    /// <param name="childIndex"></param>
    /// <returns></returns>
    public override bool RayCast(
        out RayCastOutput output,
        in RayCastInput input,
        in Transform transform,
        int childIndex)
    {
        output = default;

        // Put the ray into the polygon's frame of reference.
        Vector2 p1 = MathUtils.MulT(transform.Rotation, input.P1 - transform.Position);
        Vector2 p2 = MathUtils.MulT(transform.Rotation, input.P2 - transform.Position);
        Vector2 d = p2 - p1;

        float lower = 0.0f, upper = input.MaxFraction;

        int index = -1;

        for (int i = 0; i < this.Count; ++i)
        {
            // p = p1 + a * d
            // dot(normal, p - v) = 0
            // dot(normal, p1 - v) + a * dot(normal, d) = 0
            float numerator = Vector2.Dot(this.Normals[i], this.Vertices[i] - p1);
            float denominator = Vector2.Dot(this.Normals[i], d);

            if (denominator.Equals(0.0f))
            {
                if (numerator < 0.0f)
                {
                    return false;
                }
            }
            else
            {
                // Note: we want this predicate without division:
                // lower < numerator / denominator, where denominator < 0
                // Since denominator < 0, we have to flip the inequality:
                // lower < numerator / denominator <==> denominator * lower > numerator.
                if (denominator < 0.0f && numerator < lower * denominator)
                {
                    // Increase lower.
                    // The segment enters this half-space.
                    lower = numerator / denominator;
                    index = i;
                }
                else if (denominator > 0.0f && numerator < upper * denominator)
                {
                    // Decrease upper.
                    // The segment exits this half-space.
                    upper = numerator / denominator;
                }
            }

            // The use of epsilon here causes the assert on lower to trip
            // in some cases. Apparently the use of epsilon was to make edge
            // shapes work, but now those are handled separately.
            //if (upper < lower - b2_epsilon)
            if (upper < lower)
            {
                return false;
            }
        }

        Debug.Assert(0.0f <= lower && lower <= input.MaxFraction);

        if (index >= 0)
        {
            output = new RayCastOutput
            {
                Fraction = lower, Normal = MathUtils.Mul(transform.Rotation, this.Normals[index])
            };
            return true;
        }

        return false;
    }

    /// @see b2Shape::ComputeAABB
    public override void ComputeAABB(out AABB aabb, in Transform transform, int childIndex)
    {
        Vector2 lower = MathUtils.Mul(transform, this.Vertices[0]);
        Vector2 upper = lower;

        for (int i = 1; i < this.Count; ++i)
        {
            Vector2 v = MathUtils.Mul(transform, this.Vertices[i]);
            lower = Vector2.Min(lower, v);
            upper = Vector2.Max(upper, v);
        }

        Vector2 r = new Vector2(this.Radius, this.Radius);
        aabb = new AABB {LowerBound = lower - r, UpperBound = upper + r};
    }

    /// @see b2Shape::ComputeMass
    public override void ComputeMass(out MassData massData, float density)
    {
        // Polygon mass, centroid, and inertia.
        // Let rho be the polygon density in mass per unit area.
        // Then:
        // mass = rho * int(dA)
        // centroid.x = (1/mass) * rho * int(x * dA)
        // centroid.y = (1/mass) * rho * int(y * dA)
        // I = rho * int((x*x + y*y) * dA)
        //
        // We can compute these integrals by summing all the integrals
        // for each triangle of the polygon. To evaluate the integral
        // for a single triangle, we make a change of variables to
        // the (u,v) coordinates of the triangle:
        // x = x0 + e1x * u + e2x * v
        // y = y0 + e1y * u + e2y * v
        // where 0 <= u && 0 <= v && u + v <= 1.
        //
        // We integrate u from [0,1-v] and then v from [0,1].
        // We also need to use the Jacobian of the transformation:
        // D = cross(e1, e2)
        //
        // Simplification: triangle centroid = (1/3) * (p1 + p2 + p3)
        //
        // The rest of the derivation is handled by computer algebra.

        Debug.Assert(this.Count >= 3);

        Vector2 center = new Vector2(0.0f, 0.0f);
        float area = 0.0f;
        float I = 0.0f;

        // Get a reference point for forming triangles.
        // Use the first vertex to reduce round-off errors.
        ref readonly Vector2 s = ref this.Vertices[0];
        const float k_inv3 = 1.0f / 3.0f;

        for (int i = 0; i < this.Count; ++i)
        {
            // Triangle vertices.
            Vector2 e1 = this.Vertices[i] - s;
            Vector2 e2 = i + 1 < this.Count ? this.Vertices[i + 1] - s : this.Vertices[0] - s;

            float D = MathUtils.Cross(e1, e2);

            float triangleArea = 0.5f * D;
            area += triangleArea;

            // Area weighted centroid
            center += triangleArea * k_inv3 * (e1 + e2);

            float ex1 = e1.X, ey1 = e1.Y;
            float ex2 = e2.X, ey2 = e2.Y;

            float intx2 = ex1 * ex1 + ex2 * ex1 + ex2 * ex2;
            float inty2 = ey1 * ey1 + ey2 * ey1 + ey2 * ey2;

            I += 0.25f * k_inv3 * D * (intx2 + inty2);
        }

        // Total mass
        massData = new MassData {Mass = density * area};

        // Center of mass
        Debug.Assert(area > Settings.Epsilon);
        center *= 1.0f / area;
        massData.Center = center + s;

        // Inertia tensor relative to the local origin (point s).
        massData.RotationInertia = density * I;

        // Shift to center of mass then to original body origin.
        massData.RotationInertia += massData.Mass
                                    * (Vector2.Dot(massData.Center, massData.Center)
                                       - Vector2.Dot(center, center));
    }

    /// Validate convexity. This is a very time consuming operation.
    /// @returns true if valid
    public bool Validate()
    {
        for (int i = 0; i < this.Count; ++i)
        {
            int i1 = i;
            int i2 = i < this.Count - 1 ? i1 + 1 : 0;
            Vector2 p = this.Vertices[i1];
            Vector2 e = this.Vertices[i2] - p;

            for (int j = 0; j < this.Count; ++j)
            {
                if (j == i1 || j == i2)
                {
                    continue;
                }

                Vector2 v = this.Vertices[j] - p;
                float c = MathUtils.Cross(e, v);
                if (c < 0.0f)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static Vector2 ComputeCentroid(in Vector2[] vs, int count)
    {
        Debug.Assert(count >= 3);

        Vector2 c = new Vector2(0.0f, 0.0f);
        float area = 0.0f;

        // Get a reference point for forming triangles.
        // Use the first vertex to reduce round-off errors.
        Vector2 s = vs[0];

        const float inv3 = 1.0f / 3.0f;

        for (int i = 0; i < count; ++i)
        {
            // Triangle vertices.
            Vector2 p1 = vs[0] - s;
            Vector2 p2 = vs[i] - s;
            Vector2 p3 = i + 1 < count ? vs[i + 1] - s : vs[0] - s;

            Vector2 e1 = p2 - p1;
            Vector2 e2 = p3 - p1;

            float D = MathUtils.Cross(e1, e2);

            float triangleArea = 0.5f * D;
            area += triangleArea;

            // Area weighted centroid
            c += triangleArea * inv3 * (p1 + p2 + p3);
        }

        // Centroid
        Debug.Assert(area > Settings.Epsilon);
        c = (1.0f / area) * c + s;
        return c;
    }
}