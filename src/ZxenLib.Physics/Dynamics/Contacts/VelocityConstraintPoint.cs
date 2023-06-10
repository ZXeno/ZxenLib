namespace ZxenLib.Physics.Dynamics.Contacts;

using Microsoft.Xna.Framework;

public struct VelocityConstraintPoint
{
    public float NormalImpulse;

    public float NormalMass;

    public Vector2 Ra;

    public Vector2 Rb;

    public float TangentImpulse;

    public float TangentMass;

    public float VelocityBias;
}