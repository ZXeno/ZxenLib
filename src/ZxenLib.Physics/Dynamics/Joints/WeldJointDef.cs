namespace ZxenLib.Physics.Dynamics.Joints;

using Microsoft.Xna.Framework;
using Common;

/// Weld joint definition. You need to specify local anchor points
/// where they are attached and the relative body angle. The position
/// of the anchor points is important for computing the reaction torque.
public class WeldJointDef : JointDef
{
    /// The rotational stiffness in N*m
    /// Disable softness with a value of 0
    public float Stiffness;

    /// The rotational damping in N*m*s
    public float Damping;

    /// The local anchor point relative to bodyA's origin.
    public Vector2 LocalAnchorA;

    /// The local anchor point relative to bodyB's origin.
    public Vector2 LocalAnchorB;

    /// The bodyB angle minus bodyA angle in the reference state (radians).
    public float ReferenceAngle;

    public WeldJointDef()
    {
        this.JointType = JointType.WeldJoint;
        this.LocalAnchorA.Set(0.0f, 0.0f);
        this.LocalAnchorB.Set(0.0f, 0.0f);
        this.ReferenceAngle = 0.0f;
        this.Stiffness = 0.0f;
        this.Damping = 0.0f;
    }

    /// <summary>
    /// Initialize the bodies, anchors, reference angle, stiffness, and damping.
    /// </summary>
    /// <param name="bA">the first body connected by this joint</param>
    /// <param name="bB">the second body connected by this joint</param>
    /// <param name="anchor">the point of connection in world coordinates</param>
    public void Initialize(Body bA, Body bB, in Vector2 anchor)
    {
        this.BodyA = bA;
        this.BodyB = bB;
        this.LocalAnchorA = this.BodyA.GetLocalPoint(anchor);
        this.LocalAnchorB = this.BodyB.GetLocalPoint(anchor);
        this.ReferenceAngle = this.BodyB.GetAngle() - this.BodyA.GetAngle();
    }
}