namespace ZxenLib.Physics;

using System;
using Microsoft.Xna.Framework;

/// <summary>
/// Help with very basic physics and collision detections.
/// </summary>
public static class PhysicsHelper
{
    /// <summary>
    /// Gets the normal for a given vector.
    /// </summary>
    /// <param name="vector">The vector to normalize.</param>
    /// <returns><see cref="Vector2"/> normal.</returns>
    public static Vector2 Normal(Vector2 vector)
    {
        return new Vector2(vector.Y, -vector.X);
    }

    /// <summary>
    /// Gets the normals of a provided array of verticies.
    /// </summary>
    /// <param name="verticies">The verticies with which to derive an array of normals.</param>
    /// <returns><see cref="Vector2[]"/> containing a set of normals for the provided verticies.</returns>
    public static Vector2[] GetEdgeNormals(Vector2[] verticies)
    {
        if (verticies == null || verticies.Length == 0)
        {
            throw new ArgumentException($"{nameof(verticies)} value is null or 0");
        }

        Vector2[] normals = new Vector2[verticies.Length];
        for (int x = 0; x < verticies.Length; x++)
        {
            int v2index = x + 1;
            if (v2index >= verticies.Length)
            {
                v2index = 0;
            }

            Vector2 v1 = verticies[x];
            Vector2 v2 = verticies[v2index];

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
        Vector2[] rect1Verts = rect1.GetVerticies();
        Vector2[] rect2Verts = rect2.GetVerticies();
        Vector2[] rect1Normals = PhysicsHelper.GetEdgeNormals(rect1Verts);
        Vector2[] rect2Normals = PhysicsHelper.GetEdgeNormals(rect2Verts);
        Vector2[] allNormals = new Vector2[rect1Normals.Length + rect2Normals.Length]; // new List<Vector2>(rect1Normals.Count + rect2Normals.Count);
        rect1Normals.CopyTo(allNormals, 0);
        rect2Normals.CopyTo(allNormals, rect1Normals.Length);
        float overlap = float.MaxValue;
        Vector2 minPenetrationAxis = Vector2.Zero;

        for (int x = 0; x < rect1Normals.Length; x++)
        {
            Vector2 axis = rect1Normals[x];

            // Project onto current axis
            Tuple<float, float> rect1Projection = PhysicsHelper.ProjectOntoAxis(rect1Verts, axis);
            Tuple<float, float> rect2Projection = PhysicsHelper.ProjectOntoAxis(rect2Verts, axis);

            // check for no overlap
            if (rect1Projection.Item1 > rect2Projection.Item2
                || rect2Projection.Item1 > rect1Projection.Item2)
            {
                return null;
            }

            // otherwise, get overlap
            float o = Math.Min(rect1Projection.Item2, rect2Projection.Item2) - Math.Max(rect1Projection.Item1, rect2Projection.Item1);
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
    /// Projects an array of vertexes onto an axis.
    /// </summary>
    /// <param name="verts">The vertexes to project.</param>
    /// <param name="axis">The axis to project them on.</param>
    /// <returns>A <see cref="Tuple{float, float}"/> where Item1 is is the minimum projecttion, and Item2 is the maximum projection.</returns>
    private static Tuple<float, float> ProjectOntoAxis(Vector2[] verts, Vector2 axis)
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

        return new Tuple<float, float>(min, max);
    }
}