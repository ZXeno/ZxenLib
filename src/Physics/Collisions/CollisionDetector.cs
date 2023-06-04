namespace ZxenLib.Physics.Collisions;

using System;
using System.Diagnostics;
using Common;
using Interfaces;
using Microsoft.Xna.Framework;
using ZxenLib.Extensions;
using ZxenLib.Physics.Primitives;

public class CollisionDetector
{
    public static CollisionManifold FindCollisionFeatures(Circle a, Circle b)
    {
        float radiiSum = a.Radius + b.Radius;
        Vector2 distVec = b.Position - a.Position;
        if (distVec.LengthSquared() - radiiSum.Sqr() > 0)
        {
            return new();
        }

        // TODO: Account for momentum/velocity
        float depth = Math.Abs(distVec.Length() - radiiSum) * .5f;
        Vector2 normal = distVec;
        normal.Normalize();
        float distToPoint = a.Radius - depth;
        Vector2 contactPoint = a.Position + normal * distToPoint;
        return new()
        {
            Normal = normal,
            Depth = depth,
            ContactPoints = new []{contactPoint},
        };
    }

    public static CollisionManifold FindCollisionFeatures(Circle circle, IPolygon2D polygon)
    {
        CollisionManifold manifold = new();
        PhysicsTransform? xfA = circle.Rigidbody?.PhysicsTransform;
        PhysicsTransform? xfB = polygon.Rigidbody?.PhysicsTransform;

        Debug.Assert(xfA != null, "The rigidbody object must not be null here.");
        Debug.Assert(xfB != null, "The rigidbody object must not be null here.");

        // Compute circle position in the frame of the polygon.
        Vector2 c = PhysicsTransform.Mul(xfB, circle.Position);
        Vector2 cLocal = PhysicsTransform.MulT(xfA, c);

        // Find the min separating edge.
        int normalIndex = 0;
        float separation = -float.MaxValue;
        float radius = polygon.Radius + circle.Radius;
        int vertexCount = polygon.VertexCount;
        Span<Vector2> vertices = polygon.GetVertices();
        Span<Vector2> normals =  PhysicsHelper.GetEdgeNormals(vertices);

        for (int i = 0; i < vertexCount; ++i)
        {
            float s = Vector2.Dot(normals[i], cLocal - vertices[i]);

            if (s > radius)
            {
                // Early out.
                return manifold;
            }

            if (s > separation)
            {
                separation = s;
                normalIndex = i;
            }
        }

        // Vertices that subtend the incident face.
        int vertIndex1 = normalIndex;
        int vertIndex2 = vertIndex1 + 1 < vertexCount ? vertIndex1 + 1 : 0;
        Vector2 v1 = vertices[vertIndex1];
        Vector2 v2 = vertices[vertIndex2];

        // If the center is inside the polygon ...
        if (separation < FloatExtensions.DefaultEpsilon)
        {
            manifold.Normal = normals[normalIndex];
            manifold.ContactPoints = new Vector2[]{0.5f * (v1 + v2)};

            return manifold;
        }

        // Compute barycentric coordinates
        float u1 = Vector2.Dot(cLocal - v1, v2 - v1);
        float u2 = Vector2.Dot(cLocal - v2, v1 - v2);
        if (u1 <= 0.0f)
        {
            if (Vector2.DistanceSquared(cLocal, v1) > radius * radius)
            {
                return manifold;
            }

            manifold.Normal = cLocal - v1;
            manifold.Normal.Normalize();
            manifold.ContactPoints = new []{v1};
            return manifold;
        }

        if (u2 <= 0.0f)
        {
            if (Vector2.DistanceSquared(cLocal, v2) > radius * radius)
            {
                return manifold;
            }

            manifold.Normal = cLocal - v2;
            manifold.Normal.Normalize();
            manifold.ContactPoints = new []{v2};
            return manifold;
        }

        Vector2 faceCenter = 0.5f * (v1 + v2);
        float dval = Vector2.Dot(cLocal - faceCenter, normals[vertIndex1]);
        if (dval > radius)
        {
            return manifold;
        }

        manifold.Normal = normals[vertIndex1];
        manifold.ContactPoints = new []{faceCenter};

        return manifold;
    }

    public static CollisionManifold? FindCollisionFeatures(ICollider2D col1, ICollider2D col2)
    {
        if (col1 is Circle c1 && col2 is Circle c2)
        {
            return FindCollisionFeatures(c1, c2);
        }

        return null;
    }
}