namespace ZxenLib.Physics.Dynamics.Joints;

using System;
using Microsoft.Xna.Framework;
using Common;

/// Distance joint definition. This requires defining an anchor point on both
/// bodies and the non-zero distance of the distance joint. The definition uses
/// local anchor points so that the initial configuration can violate the
/// constraint slightly. This helps when saving and loading a game.
public class DistanceJointDef : JointDef
{
    /// Minimum length. Clamped to a stable minimum value.
    public float MinLength;

    /// Maximum length. Must be greater than or equal to the minimum length.
    public float MaxLength;

    /// The linear stiffness in N/m.
    public float Stiffness;

    /// The linear damping in N*s/m.
    public float Damping;

    /// The rest length of this joint. Clamped to a stable minimum value.
    public float Length;

    /// The local anchor point relative to bodyA's origin.
    public Vector2 LocalAnchorA;

    /// The local anchor point relative to bodyB's origin.
    public Vector2 LocalAnchorB;

    public DistanceJointDef()
    {
        this.JointType = JointType.DistanceJoint;
        this.LocalAnchorA.Set(0.0f, 0.0f);
        this.LocalAnchorB.Set(0.0f, 0.0f);
        this.MinLength = 0.0f;
        this.MaxLength = Settings.MaxFloat;
        this.Length = 1.0f;
        this.Stiffness = 0.0f;
        this.Damping = 0.0f;
    }

    /// Initialize the bodies, anchors, and rest length using world space anchors.
    /// The minimum and maximum lengths are set to the rest length.
    public void Initialize(
        Body b1,
        Body b2,
        in Vector2 anchor1,
        in Vector2 anchor2)
    {
        this.BodyA = b1;
        this.BodyB = b2;
        this.LocalAnchorA = this.BodyA.GetLocalPoint(anchor1);
        this.LocalAnchorB = this.BodyB.GetLocalPoint(anchor2);
        Vector2 d = anchor2 - anchor1;
        this.Length = Math.Max(d.Length(), Settings.LinearSlop);
        this.MinLength = this.Length;
        this.MaxLength = this.Length;
    }
}