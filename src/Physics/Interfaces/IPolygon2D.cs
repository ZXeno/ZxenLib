namespace ZxenLib.Physics.Interfaces;

using System;
using Microsoft.Xna.Framework;

public interface IPolygon2D : ICollider2D
{
    Vector2 Center { get; set; }

    Vector2 Size { get; set; }

    Vector2 HalfSize { get; }

    float Rotation { get; set; }

    float Radius { get; }

    int VertexCount { get; }

    Span<Vector2> GetVertices();

    Vector2 GetLocalMin();

    Vector2 GetLocalMax();
}