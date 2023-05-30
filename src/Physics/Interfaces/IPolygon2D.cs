namespace ZxenLib.Physics.Interfaces;

using System;
using Microsoft.Xna.Framework;

public interface IPolygon2D
{
    Vector2 Center { get; set; }

    Vector2 Size { get; set; }

    Vector2 HalfSize { get; }

    Vector2 Position { get; set; }

    float Rotation { get; set; }

    Span<Vector2> GetVertices();

    Vector2 GetLocalMin();

    Vector2 GetLocalMax();
}