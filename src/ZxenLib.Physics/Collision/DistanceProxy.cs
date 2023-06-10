namespace ZxenLib.Physics.Collision;

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;
using Shapes;

/// A distance proxy is used by the GJK algorithm.
/// It encapsulates any shape.
public struct DistanceProxy
{
    /// Initialize the proxy using the given shape. The shape
    /// must remain in scope while the proxy is in use.
    public void Set(Shape shape, int index)
    {
        switch (shape)
        {
            case CircleShape circle:
            {
                this.Vertices = new[] {circle.Position};
                this.Count = 1;
                this.Radius = circle.Radius;
            }
                break;

            case PolygonShape polygon:
            {
                this.Vertices = polygon.Vertices;
                this.Count = polygon.Count;
                this.Radius = polygon.Radius;
            }
                break;

            case ChainShape chain:
            {
                Debug.Assert(0 <= index && index < chain.Count);
                this.Count = 2;
                this.Vertices = new Vector2[this.Count];
                this.Vertices[0] = chain.Vertices[index];
                if (index + 1 < chain.Count)
                {
                    this.Vertices[1] = chain.Vertices[index + 1];
                }
                else
                {
                    this.Vertices[1] = chain.Vertices[0];
                }

                this.Radius = chain.Radius;
            }
                break;

            case EdgeShape edge:
            {
                this.Vertices = new[]
                {
                    edge.Vertex1,
                    edge.Vertex2
                };
                this.Count = 2;
                this.Radius = edge.Radius;
            }
                break;

            default:
                throw new NotSupportedException();
        }
    }

    /// Initialize the proxy using a vertex cloud and radius. The vertices
    /// must remain in scope while the proxy is in use.
    public void Set(Vector2[] vertices, int count, float radius)
    {
        this.Vertices = new Vector2[vertices.Length];
        Array.Copy(vertices, this.Vertices, vertices.Length);
        this.Count = count;
        this.Radius = radius;
    }

    /// Get the supporting vertex index in the given direction.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Pure]
    public int GetSupport(in Vector2 d)
    {
        int bestIndex = 0;
        float bestValue = Vector2.Dot(this.Vertices[0], d);
        for (int i = 1; i < this.Count; ++i)
        {
            float value = Vector2.Dot(this.Vertices[i], d);
            if (value > bestValue)
            {
                bestIndex = i;
                bestValue = value;
            }
        }

        return bestIndex;
    }

    /// Get the supporting vertex in the given direction.
    public ref readonly Vector2 GetSupportVertex(in Vector2 d)
    {
        int bestIndex = 0;
        float bestValue = Vector2.Dot(this.Vertices[0], d);
        for (int i = 1; i < this.Count; ++i)
        {
            float value = Vector2.Dot(this.Vertices[i], d);
            if (value > bestValue)
            {
                bestIndex = i;
                bestValue = value;
            }
        }

        return ref this.Vertices[bestIndex];
    }

    /// Get the vertex count.
    public int GetVertexCount()
    {
        return this.Count;
    }

    /// Get a vertex by index. Used by b2Distance.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Pure]
    public ref readonly Vector2 GetVertex(int index)
    {
        Debug.Assert(0 <= index && index < this.Count);
        return ref this.Vertices[index];
    }

    public Vector2[] Vertices;

    public int Count;

    public float Radius;
}

public class GJkProfile
{
    // GJK using Voronoi regions (Christer Ericson) and Barycentric coordinates.
    public int GjkCalls;

    public int GjkIters;

    public int GjkMaxIters;
}