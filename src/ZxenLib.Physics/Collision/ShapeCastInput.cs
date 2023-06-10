namespace ZxenLib.Physics.Collision;

using Microsoft.Xna.Framework;
using Common;

/// Input parameters for b2ShapeCast
public struct ShapeCastInput
{
    public DistanceProxy ProxyA;

    public DistanceProxy ProxyB;

    public Transform TransformA;

    public Transform TransformB;

    public Vector2 TranslationB;
}