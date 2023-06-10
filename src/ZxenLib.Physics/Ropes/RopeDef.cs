namespace ZxenLib.Physics.Ropes;

using Microsoft.Xna.Framework;

///
public struct RopeDef
{
    public Vector2 Position;

    public Vector2[] Vertices;

    public int Count;

    public float[] Masses;

    public Vector2 Gravity;

    public RopeTuning Tuning;
};