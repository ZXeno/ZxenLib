namespace ZxenLib.Physics.Dynamics.Joints;

using System.Diagnostics;
using Microsoft.Xna.Framework;
using Common;

/// Pulley joint definition. This requires two ground anchors,
/// two dynamic body anchor points, and a pulley ratio.
public class PulleyJointDef : JointDef
{
    /// The first ground anchor in world coordinates. This point never moves.
    public Vector2 GroundAnchorA;

    /// The second ground anchor in world coordinates. This point never moves.
    public Vector2 GroundAnchorB;

    /// The a reference length for the segment attached to bodyA.
    public float LengthA;

    /// The a reference length for the segment attached to bodyB.
    public float LengthB;

    /// The local anchor point relative to bodyA's origin.
    public Vector2 LocalAnchorA;

    /// The local anchor point relative to bodyB's origin.
    public Vector2 LocalAnchorB;

    /// The pulley ratio, used to simulate a block-and-tackle.
    public float Ratio;

    public PulleyJointDef()
    {
        this.JointType = JointType.PulleyJoint;

        this.GroundAnchorA.Set(-1.0f, 1.0f);

        this.GroundAnchorB.Set(1.0f, 1.0f);

        this.LocalAnchorA.Set(-1.0f, 0.0f);

        this.LocalAnchorB.Set(1.0f, 0.0f);

        this.LengthA = 0.0f;

        this.LengthB = 0.0f;

        this.Ratio = 1.0f;

        this.CollideConnected = true;
    }

    /// Initialize the bodies, anchors, lengths, max lengths, and ratio using the world anchors.
    public void Initialize(
        Body bA,
        Body bB,
        in Vector2 groundA,
        in Vector2 groundB,
        in Vector2 anchorA,
        in Vector2 anchorB,
        float r)
    {
        this.BodyA = bA;
        this.BodyB = bB;
        this.GroundAnchorA = groundA;
        this.GroundAnchorB = groundB;
        this.LocalAnchorA = this.BodyA.GetLocalPoint(anchorA);
        this.LocalAnchorB = this.BodyB.GetLocalPoint(anchorB);
        Vector2 dA = anchorA - groundA;
        this.LengthA = dA.Length();
        Vector2 dB = anchorB - groundB;
        this.LengthB = dB.Length();
        this.Ratio = r;
        Debug.Assert(this.Ratio > Settings.Epsilon);
    }
}