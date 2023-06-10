namespace ZxenLib.Physics.Dynamics.Joints;

using Microsoft.Xna.Framework;
using Common;

/// Mouse joint definition. This requires a world target point,
/// tuning parameters, and the time step.
public class MouseJointDef : JointDef
{
    /// The damping ratio. 0 = no damping, 1 = critical damping.
    public float Damping;

    /// The response speed.
    public float Stiffness;

    /// The maximum constraint force that can be exerted
    /// to move the candidate body. Usually you will express
    /// as some multiple of the weight (multiplier * mass * gravity).
    public float MaxForce;

    /// The initial world target point. This is assumed
    /// to coincide with the body anchor initially.
    public Vector2 Target;

    public MouseJointDef()
    {
        this.JointType = JointType.MouseJoint;
        this.Target.Set(0.0f, 0.0f);
        this.MaxForce = 0.0f;
        this.Stiffness = 5.0f;
        this.Damping = 0.7f;
    }
}