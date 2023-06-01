namespace ZxenLib.Physics.Interfaces;

using System;
using Microsoft.Xna.Framework;

public interface IPolygon2D : IShape
{
    Vector2 Center { get; set; }

    Vector2 Size { get; set; }

    Vector2 HalfSize { get; }

    float Rotation { get; set; }

    Span<Vector2> GetVertices();

    Vector2 GetLocalMin();

    Vector2 GetLocalMax();
}