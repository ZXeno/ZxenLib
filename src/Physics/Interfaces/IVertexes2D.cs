namespace ZxenLib.Physics.Interfaces;

using System;
using Microsoft.Xna.Framework;

public interface IVertexes2D
{
    Span<Vector2> GetVertices();
}