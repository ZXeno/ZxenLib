namespace ZxenLib.Physics.Dynamics.Joints;

using Microsoft.Xna.Framework;
using Common;

/// Revolute joint definition. This requires defining an anchor point where the
/// bodies are joined. The definition uses local anchor points so that the
/// initial configuration can violate the constraint slightly. You also need to
/// specify the initial relative angle for joint limits. This helps when saving
/// and loading a game.
/// The local anchor points are measured from the body's origin
/// rather than the center of mass because:
/// 1. you might not know where the center of mass will be.
/// 2. if you add/remove shapes from a body and recompute the mass,
///    the joints will be broken.
public class RevoluteJointDef : JointDef
{
    /// A flag to enable joint limits.
    public bool EnableLimit;

    /// A flag to enable the joint motor.
    public bool EnableMotor;

    /// The local anchor point relative to bodyA's origin.
    public Vector2 LocalAnchorA;

    /// The local anchor point relative to bodyB's origin.
    public Vector2 LocalAnchorB;

    /// The lower angle for the joint limit (radians).
    public float LowerAngle;

    /// The maximum motor torque used to achieve the desired motor speed.
    /// Usually in N-m.
    public float MaxMotorTorque;

    /// The desired motor speed. Usually in radians per second.
    public float MotorSpeed;

    /// The bodyB angle minus bodyA angle in the reference state (radians).
    public float ReferenceAngle;

    /// The upper angle for the joint limit (radians).
    public float UpperAngle;

    public RevoluteJointDef()
    {
        this.JointType = JointType.RevoluteJoint;
        this.LocalAnchorA.Set(0.0f, 0.0f);
        this.LocalAnchorB.Set(0.0f, 0.0f);
        this.ReferenceAngle = 0.0f;
        this.LowerAngle = 0.0f;
        this.UpperAngle = 0.0f;
        this.MaxMotorTorque = 0.0f;
        this.MotorSpeed = 0.0f;
        this.EnableLimit = false;
        this.EnableMotor = false;
    }

    /// Initialize the bodies, anchors, and reference angle using a world
    /// anchor point.
    public void Initialize(Body bA, Body bB, Vector2 anchor)
    {
        this.BodyA = bA;
        this.BodyB = bB;
        this.LocalAnchorA = this.BodyA.GetLocalPoint(anchor);
        this.LocalAnchorB = this.BodyB.GetLocalPoint(anchor);
        this.ReferenceAngle = this.BodyB.GetAngle() - this.BodyA.GetAngle();
    }
}