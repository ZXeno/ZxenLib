namespace ZxenLib.Physics;

using System;
using Extensions;
using Microsoft.Xna.Framework;

/// <summary>
/// Help with very basic physics and collision detections.
/// </summary>
public static class PhysicsHelper
{
    /// <summary>
    /// Gets the normal for a given vector.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns><see cref="Vector2"/> normal.</returns>
    public static Vector2 Normal(Vector2 vector)
    {
        return new Vector2(vector.Y, -vector.X);
    }

    /// <summary>
    /// Gets the normals of a provided array of vertices.
    /// </summary>
    /// <param name="vertices">The vertices with which to derive an array of normals.</param>
    /// <returns><see cref="Vector2"/> array containing a set of normals for the provided vertices.</returns>
    public static Span<Vector2> GetEdgeNormals(Span<Vector2> vertices)
    {
        if (vertices == null || vertices.Length == 0)
        {
            throw new ArgumentException($"{nameof(vertices)} value is null or 0");
        }

        Span<Vector2> normals = new Span<Vector2>(new Vector2[vertices.Length]);
        for (int x = 0; x < vertices.Length; x++)
        {
            int v2index = x + 1;
            if (v2index >= vertices.Length)
            {
                v2index = 0;
            }

            Vector2 v1 = vertices[x];
            Vector2 v2 = vertices[v2index];

            Vector2 edgeVector = v1 - v2;
            Vector2 edgeNormal = PhysicsHelper.Normal(edgeVector);
            edgeNormal.Normalize();

            normals[x] = new Vector2(edgeNormal.X, edgeNormal.Y);
        }

        return normals;
    }

    /// <summary>
    /// Checks for collision between two rectangles using Separating Axis Theorem.
    /// </summary>
    /// <param name="rect1">The first rectangle to check.</param>
    /// <param name="rect2">The second rectangle to check.</param>
    /// <returns>Minimum Penetration Axis as nullable <see cref="Vector2"/>.</returns>
    public static Vector2? SatCollisionDetect(Rectangle rect1, Rectangle rect2)
    {
        Span<Vector2> rect1Verts = rect1.GetVertices();
        Span<Vector2> rect2Verts = rect2.GetVertices();
        Span<Vector2> rect1Normals = PhysicsHelper.GetEdgeNormals(rect1Verts);
        Span<Vector2> rect2Normals = PhysicsHelper.GetEdgeNormals(rect2Verts);
        Vector2[] allNormals = new Vector2[rect1Normals.Length + rect2Normals.Length];
        rect1Normals.CopyTo(allNormals);
        rect2Normals.CopyTo(allNormals.AsSpan(rect1Normals.Length));
        float overlap = float.MaxValue;
        Vector2 minPenetrationAxis = Vector2.Zero;

        for (int x = 0; x < rect1Normals.Length; x++)
        {
            Vector2 axis = rect1Normals[x];

            // Project onto current axis
            Span<float> rect1Projection = PhysicsHelper.ProjectOntoAxis(rect1Verts, axis);
            Span<float> rect2Projection = PhysicsHelper.ProjectOntoAxis(rect2Verts, axis);

            // check for no overlap
            if (rect1Projection[0] > rect2Projection[1]
                || rect2Projection[0] > rect1Projection[1])
            {
                return null;
            }

            // otherwise, get overlap
            float o = Math.Min(rect1Projection[1], rect2Projection[1]) - Math.Max(rect1Projection[0], rect2Projection[0]);
            if (o < overlap)
            {
                overlap = o;
                minPenetrationAxis = axis;
            }
        }

        // if we get here, we're colliding...
        return minPenetrationAxis;
    }

    /// <summary>
    /// Projects an array of vertices onto an axis.
    /// </summary>
    /// <param name="verts">The vertexes to project.</param>
    /// <param name="axis">The axis to project them on.</param>
    /// <returns>A <see cref="Span{float}"/> where index 0 is is the minimum projection, and index 1 is the maximum projection.</returns>
    public static Span<float> ProjectOntoAxis(Span<Vector2> verts, Vector2 axis)
    {
        float min = Vector2.Dot(axis, verts[0]);
        float max = min;

        for (int x = 1; x < verts.Length; x++)
        {
            float projection = Vector2.Dot(axis, verts[x]);
            if (projection < min)
            {
                min = projection;
            }
            else if (projection > max)
            {
                max = projection;
            }
        }

        return new float[] { min, max };
    }
}