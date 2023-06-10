namespace ZxenLib.Physics.Dynamics.Joints;

using Microsoft.Xna.Framework;
using Common;

/// Motor joint definition.
public class MotorJointDef : JointDef
{
    /// The bodyB angle minus bodyA angle in radians.
    public float AngularOffset;

    /// Position correction factor in the range [0,1].
    public float CorrectionFactor;

    /// Position of bodyB minus the position of bodyA, in bodyA's frame, in meters.
    public Vector2 LinearOffset;

    /// The maximum motor force in N.
    public float MaxForce;

    /// The maximum motor torque in N-m.
    public float MaxTorque;

    public MotorJointDef()
    {
        this.JointType = JointType.MotorJoint;
        this.LinearOffset.SetZero();
        this.AngularOffset = 0.0f;
        this.MaxForce = 1.0f;
        this.MaxTorque = 1.0f;
        this.CorrectionFactor = 0.3f;
    }

    /// Initialize the bodies and offsets using the current transforms.
    public void Initialize(Body bA, Body bB)
    {
        this.BodyA = bA;
        this.BodyB = bB;
        Vector2 xB = this.BodyB.GetPosition();
        this.LinearOffset = this.BodyA.GetLocalPoint(xB);

        float angleA = this.BodyA.GetAngle();
        float angleB = this.BodyB.GetAngle();
        this.AngularOffset = angleB - angleA;
    }
}