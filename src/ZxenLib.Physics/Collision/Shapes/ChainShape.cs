namespace ZxenLib.Physics.Collision.Shapes;

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Collider;
using Common;

/// <summary>
/// A chain shape is a free form sequence of line segments.
/// The chain has one-sided collision, with the surface normal pointing to the right of the edge.
/// This provides a counter-clockwise winding like the polygon shape.
/// Connectivity information is used to create smooth collisions.
/// <para>@warning the chain will not collide properly if there are self-intersections.</para>
/// </summary>
public class ChainShape : Shape
{
    /// The vertex count.
    public int Count;

    public Vector2 PrevVertex;

    public Vector2 NextVertex;

    /// The vertices. Owned by this class.
    public Vector2[] Vertices;

    public ChainShape()
    {
        this.ShapeType = ShapeType.Chain;
        this.Radius = Settings.PolygonRadius;
        this.Vertices = null;
        this.Count = 0;
    }

    /// Implement b2Shape. Vertices are cloned using b2Alloc.
    public override Shape Clone()
    {
        ChainShape? clone = new ChainShape {Vertices = new Vector2[this.Vertices.Length]};
        Array.Copy(this.Vertices, clone.Vertices, this.Vertices.Length);
        clone.Count = this.Count;
        clone.PrevVertex = this.PrevVertex;
        clone.NextVertex = this.NextVertex;
        return clone;
    }

    /// <summary>
    /// Clear all data.
    /// </summary>
    public void Clear()
    {
        this.Vertices = null;
        this.Count = 0;
    }

    /// Create a loop. This automatically adjusts connectivity.
    /// @param vertices an array of vertices, these are copied
    /// @param count the vertex count
    public void CreateLoop(Vector2[] vertices, int count = -1)
    {
        if (count == -1)
        {
            count = vertices.Length;
        }

        Debug.Assert(this.Vertices == null && this.Count == 0);
        Debug.Assert(count >= 3);
        if (count < 3)
        {
            return;
        }

        for (int i = 1; i < count; ++i)
        {
            Vector2 v1 = vertices[i - 1];
            Vector2 v2 = vertices[i];

            // If the code crashes here, it means your vertices are too close together.
            Debug.Assert(Vector2.DistanceSquared(v1, v2) > Settings.LinearSlop * Settings.LinearSlop);
        }

        this.Count = count + 1;
        this.Vertices = new Vector2[this.Count];
        Array.Copy(vertices, this.Vertices, count);
        this.Vertices[count] = this.Vertices[0];
        this.PrevVertex = this.Vertices[this.Count - 2];
        this.NextVertex = this.Vertices[1];
    }

    /// <summary>
    /// Create a chain with ghost vertices to connect multiple chains together.
    /// </summary>
    /// <param name="vertices">an array of vertices, these are copied</param>
    /// <param name="count">the vertex count</param>
    /// <param name="prevVertex">previous vertex from chain that connects to the start</param>
    /// <param name="nextVertex">next vertex from chain that connects to the end</param>
    public void CreateChain(Vector2[] vertices, int count, Vector2 prevVertex, Vector2 nextVertex)
    {
        Debug.Assert(this.Vertices == null && this.Count == 0);
        Debug.Assert(count >= 2);
        for (int i = 1; i < count; ++i)
        {
            // If the code crashes here, it means your vertices are too close together.
            Debug.Assert(
                Vector2.DistanceSquared(vertices[i - 1], vertices[i])
                > Settings.LinearSlop * Settings.LinearSlop);
        }

        this.Count = count;
        this.Vertices = new Vector2[count];
        Array.Copy(vertices, this.Vertices, count);

        this.PrevVertex = prevVertex;
        this.NextVertex = nextVertex;
    }

    /// @see b2Shape::GetChildCount
    public override int GetChildCount()
    {
        return this.Count - 1;
    }

    /// Get a child edge.
    public void GetChildEdge(out EdgeShape edge, int index)
    {
        Debug.Assert(0 <= index && index < this.Count - 1);
        edge = new EdgeShape
        {
            ShapeType = ShapeType.Edge,
            Radius = this.Radius,
            Vertex1 = this.Vertices[index + 0],
            Vertex2 = this.Vertices[index + 1],
            OneSided = true,
            Vertex0 = index > 0 ? this.Vertices[index - 1] : this.PrevVertex,
            Vertex3 = index < this.Count - 2 ? this.Vertices[index + 2] : this.NextVertex
        };
    }

    /// This always return false.
    /// @see b2Shape::TestPoint
    public override bool TestPoint(in Transform transform, in Vector2 p)
    {
        return false;
    }

    /// Implement b2Shape.
    public override bool RayCast(
        out RayCastOutput output,
        in RayCastInput input,
        in Transform transform,
        int childIndex)
    {
        Debug.Assert(childIndex < this.Count);

        EdgeShape? edgeShape = new EdgeShape();

        int i1 = childIndex;
        int i2 = childIndex + 1;
        if (i2 == this.Count)
        {
            i2 = 0;
        }

        edgeShape.Vertex1 = this.Vertices[i1];
        edgeShape.Vertex2 = this.Vertices[i2];

        return edgeShape.RayCast(out output, input, transform, 0);
    }

    /// @see b2Shape::ComputeAABB
    public override void ComputeAABB(out AABB aabb, in Transform transform, int childIndex)
    {
        Debug.Assert(childIndex < this.Count);

        int i1 = childIndex;
        int i2 = childIndex + 1;
        if (i2 == this.Count)
        {
            i2 = 0;
        }

        Vector2 v1 = MathUtils.Mul(transform, this.Vertices[i1]);
        Vector2 v2 = MathUtils.Mul(transform, this.Vertices[i2]);

        Vector2 lower = Vector2.Min(v1, v2);
        Vector2 upper = Vector2.Max(v1, v2);

        Vector2 r = new Vector2(this.Radius, this.Radius);
        aabb = new AABB(lower - r, upper + r);
    }

    /// Chains have zero mass.
    /// @see b2Shape::ComputeMass
    public override void ComputeMass(out MassData massData, float density)
    {
        massData = new MassData();
        massData.Mass = 0.0f;
        massData.Center.SetZero();
        massData.RotationInertia = 0.0f;
    }
}