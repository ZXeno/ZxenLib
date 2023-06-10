namespace ZxenLib.Physics.Collision;

using Microsoft.Xna.Framework;
using Collider;
using Common;
using Shapes;

public static partial class CollisionUtils
{
    /// An axis aligned bounding box.
    /// Compute the collision manifold between two circles.
    public static void CollideCircles(
        ref Manifold manifold,
        CircleShape circleA,
        in Transform xfA,
        CircleShape circleB,
        in Transform xfB)
    {
        manifold.PointCount = 0;

        Vector2 pA = MathUtils.Mul(xfA, circleA.Position);
        Vector2 pB = MathUtils.Mul(xfB, circleB.Position);

        Vector2 d = pB - pA;
        float distSqr = Vector2.Dot(d, d);
        float rA = circleA.Radius;
        float rB = circleB.Radius;
        float radius = rA + rB;
        if (distSqr > radius * radius)
        {
            return;
        }

        manifold.Type = ManifoldType.Circles;
        manifold.LocalPoint = circleA.Position;
        manifold.LocalNormal.SetZero();
        manifold.PointCount = 1;

        manifold.Points.Value0.LocalPoint = circleB.Position;
        manifold.Points.Value0.Id.Key = 0;
    }

    /// Compute the collision manifold between a polygon and a circle.
    public static void CollidePolygonAndCircle(
        ref Manifold manifold,
        PolygonShape polygonA,
        in Transform xfA,
        CircleShape circleB,
        in Transform xfB)
    {
        manifold.PointCount = 0;

        // Compute circle position in the frame of the polygon.
        Vector2 c = MathUtils.Mul(xfB, circleB.Position);
        Vector2 cLocal = MathUtils.MulT(xfA, c);

        // Find the min separating edge.
        int normalIndex = 0;
        float separation = -Settings.MaxFloat;
        float radius = polygonA.Radius + circleB.Radius;
        int vertexCount = polygonA.Count;
        Vector2[] vertices = polygonA.Vertices;
        Vector2[] normals = polygonA.Normals;

        for (int i = 0; i < vertexCount; ++i)
        {
            float s = Vector2.Dot(normals[i], cLocal - vertices[i]);

            if (s > radius)
            {
                // Early out.
                return;
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
        if (separation < Settings.Epsilon)
        {
            manifold.PointCount = 1;
            manifold.Type = ManifoldType.FaceA;
            manifold.LocalNormal = normals[normalIndex];
            manifold.LocalPoint = 0.5f * (v1 + v2);

            manifold.Points.Value0.LocalPoint = circleB.Position;
            manifold.Points.Value0.Id.Key = 0;
            return;
        }

        // Compute barycentric coordinates
        float u1 = Vector2.Dot(cLocal - v1, v2 - v1);
        float u2 = Vector2.Dot(cLocal - v2, v1 - v2);
        if (u1 <= 0.0f)
        {
            if (Vector2.DistanceSquared(cLocal, v1) > radius * radius)
            {
                return;
            }

            manifold.PointCount = 1;
            manifold.Type = ManifoldType.FaceA;
            manifold.LocalNormal = cLocal - v1;
            manifold.LocalNormal.Normalize();
            manifold.LocalPoint = v1;

            manifold.Points.Value0.LocalPoint = circleB.Position;
            manifold.Points.Value0.Id.Key = 0;
        }
        else if (u2 <= 0.0f)
        {
            if (Vector2.DistanceSquared(cLocal, v2) > radius * radius)
            {
                return;
            }

            manifold.PointCount = 1;
            manifold.Type = ManifoldType.FaceA;
            manifold.LocalNormal = cLocal - v2;
            manifold.LocalNormal.Normalize();
            manifold.LocalPoint = v2;

            manifold.Points.Value0.LocalPoint = circleB.Position;
            manifold.Points.Value0.Id.Key = 0;
        }
        else
        {
            Vector2 faceCenter = 0.5f * (v1 + v2);
            float s = Vector2.Dot(cLocal - faceCenter, normals[vertIndex1]);
            if (s > radius)
            {
                return;
            }

            manifold.PointCount = 1;
            manifold.Type = ManifoldType.FaceA;
            manifold.LocalNormal = normals[vertIndex1];
            manifold.LocalPoint = faceCenter;

            manifold.Points.Value0.LocalPoint = circleB.Position;
            manifold.Points.Value0.Id.Key = 0;
        }
    }
}