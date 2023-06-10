namespace ZxenLib.Physics.Dynamics.Joints;

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Common;

/// The pulley joint is connected to two bodies and two fixed ground points.
/// The pulley supports a ratio such that:
/// length1 + ratio * length2 <= constant
/// Yes, the force transmitted is scaled by the ratio.
/// Warning: the pulley joint can get a bit squirrelly by itself. They often
/// work better when combined with prismatic joints. You should also cover the
/// the anchor points with static shapes to prevent one side from going to
/// zero length.
public class PulleyJoint : Joint
{
    private readonly float _constant;

    private readonly float _lengthA;

    private readonly float _lengthB;

    // Solver shared
    private readonly Vector2 _localAnchorA;

    private readonly Vector2 _localAnchorB;

    private readonly float _ratio;

    private Vector2 _groundAnchorA;

    private Vector2 _groundAnchorB;

    private float _impulse;

    // Solver temp
    private int _indexA;

    private int _indexB;

    private float _invIa;

    private float _invIb;

    private float _invMassA;

    private float _invMassB;

    private Vector2 _localCenterA;

    private Vector2 _localCenterB;

    private float _mass;

    private Vector2 _rA;

    private Vector2 _rB;

    private Vector2 _uA;

    private Vector2 _uB;

    public PulleyJoint(PulleyJointDef def) : base(def)
    {
        this._groundAnchorA = def.GroundAnchorA;
        this._groundAnchorB = def.GroundAnchorB;
        this._localAnchorA = def.LocalAnchorA;
        this._localAnchorB = def.LocalAnchorB;

        this._lengthA = def.LengthA;
        this._lengthB = def.LengthB;

        Debug.Assert(!def.Ratio.Equals(0.0f));
        this._ratio = def.Ratio;

        this._constant = def.LengthA + this._ratio * def.LengthB;

        this._impulse = 0.0f;
    }

    /// Get the first ground anchor.
    public Vector2 GetGroundAnchorA()
    {
        return this._groundAnchorA;
    }

    /// Get the second ground anchor.
    public Vector2 GetGroundAnchorB()
    {
        return this._groundAnchorB;
    }

    /// Get the current length of the segment attached to bodyA.
    public float GetLengthA()
    {
        return this._lengthA;
    }

    /// Get the current length of the segment attached to bodyB.
    public float GetLengthB()
    {
        return this._lengthB;
    }

    /// Get the pulley ratio.
    public float GetRatio()
    {
        return this._ratio;
    }

    /// Get the current length of the segment attached to bodyA.
    public float GetCurrentLengthA()
    {
        Vector2 p = this.BodyA.GetWorldPoint(this._localAnchorA);
        Vector2 s = this._groundAnchorA;
        Vector2 d = p - s;
        return d.Length();
    }

    /// Get the current length of the segment attached to bodyB.
    public float GetCurrentLengthB()
    {
        Vector2 p = this.BodyB.GetWorldPoint(this._localAnchorB);
        Vector2 s = this._groundAnchorB;
        Vector2 d = p - s;
        return d.Length();
    }

    /// <inheritdoc />
    public override Vector2 GetAnchorA()
    {
        return this.BodyA.GetWorldPoint(this._localAnchorA);
    }

    /// <inheritdoc />
    public override Vector2 GetAnchorB()
    {
        return this.BodyB.GetWorldPoint(this._localAnchorB);
    }

    /// <inheritdoc />
    public override Vector2 GetReactionForce(float inv_dt)
    {
        Vector2 P = this._impulse * this._uB;
        return inv_dt * P;
    }

    /// <inheritdoc />
    public override float GetReactionTorque(float inv_dt)
    {
        return 0.0f;
    }

    /// <inheritdoc />
    internal override void InitVelocityConstraints(in SolverData data)
    {
        this._indexA = this.BodyA.IslandIndex;
        this._indexB = this.BodyB.IslandIndex;
        this._localCenterA = this.BodyA.Sweep.LocalCenter;
        this._localCenterB = this.BodyB.Sweep.LocalCenter;
        this._invMassA = this.BodyA.InvMass;
        this._invMassB = this.BodyB.InvMass;
        this._invIa = this.BodyA.InverseInertia;
        this._invIb = this.BodyB.InverseInertia;

        Vector2 cA = data.Positions[this._indexA].Center;
        float aA = data.Positions[this._indexA].Angle;
        Vector2 vA = data.Velocities[this._indexA].V;
        float wA = data.Velocities[this._indexA].W;

        Vector2 cB = data.Positions[this._indexB].Center;
        float aB = data.Positions[this._indexB].Angle;
        Vector2 vB = data.Velocities[this._indexB].V;
        float wB = data.Velocities[this._indexB].W;

        Rotation qA = new Rotation(aA);
        Rotation qB = new Rotation(aB);

        this._rA = MathUtils.Mul(qA, this._localAnchorA - this._localCenterA);
        this._rB = MathUtils.Mul(qB, this._localAnchorB - this._localCenterB);

        // Get the pulley axes.
        this._uA = cA + this._rA - this._groundAnchorA;
        this._uB = cB + this._rB - this._groundAnchorB;

        float lengthA = this._uA.Length();
        float lengthB = this._uB.Length();

        if (lengthA > 10.0f * Settings.LinearSlop)
        {
            this._uA *= 1.0f / lengthA;
        }
        else
        {
            this._uA.SetZero();
        }

        if (lengthB > 10.0f * Settings.LinearSlop)
        {
            this._uB *= 1.0f / lengthB;
        }
        else
        {
            this._uB.SetZero();
        }

        // Compute effective mass.
        float ruA = MathUtils.Cross(this._rA, this._uA);
        float ruB = MathUtils.Cross(this._rB, this._uB);

        float mA = this._invMassA + this._invIa * ruA * ruA;
        float mB = this._invMassB + this._invIb * ruB * ruB;

        this._mass = mA + this._ratio * this._ratio * mB;

        if (this._mass > 0.0f)
        {
            this._mass = 1.0f / this._mass;
        }

        if (data.Step.WarmStarting)
        {
            // Scale impulses to support variable time steps.
            this._impulse *= data.Step.DtRatio;

            // Warm starting.
            Vector2 PA = -this._impulse * this._uA;
            Vector2 PB = -this._ratio * this._impulse * this._uB;

            vA += this._invMassA * PA;
            wA += this._invIa * MathUtils.Cross(this._rA, PA);
            vB += this._invMassB * PB;
            wB += this._invIb * MathUtils.Cross(this._rB, PB);
        }
        else
        {
            this._impulse = 0.0f;
        }

        data.Velocities[this._indexA].V = vA;
        data.Velocities[this._indexA].W = wA;
        data.Velocities[this._indexB].V = vB;
        data.Velocities[this._indexB].W = wB;
    }

    /// <inheritdoc />
    internal override void SolveVelocityConstraints(in SolverData data)
    {
        Vector2 vA = data.Velocities[this._indexA].V;
        float wA = data.Velocities[this._indexA].W;
        Vector2 vB = data.Velocities[this._indexB].V;
        float wB = data.Velocities[this._indexB].W;

        Vector2 vpA = vA + MathUtils.Cross(wA, this._rA);
        Vector2 vpB = vB + MathUtils.Cross(wB, this._rB);

        float Cdot = -Vector2.Dot(this._uA, vpA) - this._ratio * Vector2.Dot(this._uB, vpB);
        float impulse = -this._mass * Cdot;
        this._impulse += impulse;

        Vector2 PA = -impulse * this._uA;
        Vector2 PB = -this._ratio * impulse * this._uB;
        vA += this._invMassA * PA;
        wA += this._invIa * MathUtils.Cross(this._rA, PA);
        vB += this._invMassB * PB;
        wB += this._invIb * MathUtils.Cross(this._rB, PB);

        data.Velocities[this._indexA].V = vA;
        data.Velocities[this._indexA].W = wA;
        data.Velocities[this._indexB].V = vB;
        data.Velocities[this._indexB].W = wB;
    }

    /// <inheritdoc />
    internal override bool SolvePositionConstraints(in SolverData data)
    {
        Vector2 cA = data.Positions[this._indexA].Center;
        float aA = data.Positions[this._indexA].Angle;
        Vector2 cB = data.Positions[this._indexB].Center;
        float aB = data.Positions[this._indexB].Angle;

        Rotation qA = new Rotation(aA);
        Rotation qB = new Rotation(aB);

        Vector2 rA = MathUtils.Mul(qA, this._localAnchorA - this._localCenterA);
        Vector2 rB = MathUtils.Mul(qB, this._localAnchorB - this._localCenterB);

        // Get the pulley axes.
        Vector2 uA = cA + rA - this._groundAnchorA;
        Vector2 uB = cB + rB - this._groundAnchorB;

        float lengthA = uA.Length();
        float lengthB = uB.Length();

        if (lengthA > 10.0f * Settings.LinearSlop)
        {
            uA *= 1.0f / lengthA;
        }
        else
        {
            uA.SetZero();
        }

        if (lengthB > 10.0f * Settings.LinearSlop)
        {
            uB *= 1.0f / lengthB;
        }
        else
        {
            uB.SetZero();
        }

        // Compute effective mass.
        float ruA = MathUtils.Cross(rA, uA);
        float ruB = MathUtils.Cross(rB, uB);

        float mA = this._invMassA + this._invIa * ruA * ruA;
        float mB = this._invMassB + this._invIb * ruB * ruB;

        float mass = mA + this._ratio * this._ratio * mB;

        if (mass > 0.0f)
        {
            mass = 1.0f / mass;
        }

        float C = this._constant - lengthA - this._ratio * lengthB;
        float linearError = Math.Abs(C);

        float impulse = -mass * C;

        Vector2 PA = -impulse * uA;
        Vector2 PB = -this._ratio * impulse * uB;

        cA += this._invMassA * PA;
        aA += this._invIa * MathUtils.Cross(rA, PA);
        cB += this._invMassB * PB;
        aB += this._invIb * MathUtils.Cross(rB, PB);

        data.Positions[this._indexA].Center = cA;
        data.Positions[this._indexA].Angle = aA;
        data.Positions[this._indexB].Center = cB;
        data.Positions[this._indexB].Angle = aB;

        return linearError < Settings.LinearSlop;
    }

    /// <inheritdoc />
    public override void Dump()
    {
        int indexA = this.BodyA.IslandIndex;
        int indexB = this.BodyB.IslandIndex;

        DumpLogger.Log("  b2PulleyJointDef jd;");
        DumpLogger.Log($"  jd.bodyA = bodies[{indexA}];");
        DumpLogger.Log($"  jd.bodyB = bodies[{indexB}];");
        DumpLogger.Log($"  jd.collideConnected = bool({this.CollideConnected});");
        DumpLogger.Log($"  jd.groundAnchorA.Set({this._groundAnchorA.X}, {this._groundAnchorA.Y});");
        DumpLogger.Log($"  jd.groundAnchorB.Set({this._groundAnchorB.X}, {this._groundAnchorB.Y});");
        DumpLogger.Log($"  jd.localAnchorA.Set({this._localAnchorA.X}, {this._localAnchorA.Y});");
        DumpLogger.Log($"  jd.localAnchorB.Set({this._localAnchorB.X}, {this._localAnchorB.Y});");
        DumpLogger.Log($"  jd.lengthA = {this._lengthA};");
        DumpLogger.Log($"  jd.lengthB = {this._lengthB};");
        DumpLogger.Log($"  jd.ratio = {this._ratio};");
        DumpLogger.Log($"  joints[{this.Index}] = m_world.CreateJoint(&jd);");
    }

    /// <inheritdoc />
    public override void ShiftOrigin(in Vector2 newOrigin)
    {
        this._groundAnchorA -= newOrigin;
        this._groundAnchorB -= newOrigin;
    }
}