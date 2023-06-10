namespace ZxenLib.Physics.Dynamics.Joints;

using System.Diagnostics;
using Microsoft.Xna.Framework;
using Common;

/// Friction joint. This is used for top-down friction.
/// It provides 2D translational friction and angular friction.
public class FrictionJoint : Joint
{
    private float _angularImpulse;

    private float _angularMass;

    // Solver temp
    private int _indexA;

    private int _indexB;

    private float _invIa;

    private float _invIb;

    private float _invMassA;

    private float _invMassB;

    // Solver shared
    private Vector2 _linearImpulse;

    private Matrix2x2 _linearMass;

    private Vector2 _localAnchorA;

    private Vector2 _localAnchorB;

    private Vector2 _localCenterA;

    private Vector2 _localCenterB;

    private float _maxForce;

    private float _maxTorque;

    private Vector2 _rA;

    private Vector2 _rB;

    internal FrictionJoint(FrictionJointDef def) : base(def)
    {
        this._localAnchorA = def.LocalAnchorA;
        this._localAnchorB = def.LocalAnchorB;

        this._linearImpulse.SetZero();
        this._angularImpulse = 0.0f;

        this._maxForce = def.MaxForce;
        this._maxTorque = def.MaxTorque;
    }

    /// Get/Set the maximum friction force in N.

    public float MaxForce
    {
        get => this._maxForce;
        set
        {
            Debug.Assert(value.IsValid() && value >= 0.0f);
            this._maxForce = value;
        }
    }

    public float MaxTorque
    {
        get => this._maxTorque;
        set
        {
            Debug.Assert(value.IsValid() && value >= 0.0f);
            this._maxTorque = value;
        }
    }

    /// The local anchor point relative to bodyA's origin.
    public Vector2 GetLocalAnchorA()
    {
        return this._localAnchorA;
    }

    /// The local anchor point relative to bodyB's origin.
    public Vector2 GetLocalAnchorB()
    {
        return this._localAnchorB;
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
        return inv_dt * this._linearImpulse;
    }

    /// <inheritdoc />
    public override float GetReactionTorque(float inv_dt)
    {
        return inv_dt * this._angularImpulse;
    }

    /// Dump joint to dmLog
    public override void Dump()
    { }

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

        float aA = data.Positions[this._indexA].Angle;
        Vector2 vA = data.Velocities[this._indexA].V;
        float wA = data.Velocities[this._indexA].W;

        float aB = data.Positions[this._indexB].Angle;
        Vector2 vB = data.Velocities[this._indexB].V;
        float wB = data.Velocities[this._indexB].W;

        Rotation qA = new Rotation(aA);
        Rotation qB = new Rotation(aB);

        // Compute the effective mass matrix.
        this._rA = MathUtils.Mul(qA, this._localAnchorA - this._localCenterA);
        this._rB = MathUtils.Mul(qB, this._localAnchorB - this._localCenterB);

        // J = [-I -r1_skew I r2_skew]
        //     [ 0       -1 0       1]
        // r_skew = [-ry; rx]

        // Matlab
        // K = [ mA+r1y^2*iA+mB+r2y^2*iB,  -r1y*iA*r1x-r2y*iB*r2x,          -r1y*iA-r2y*iB]
        //     [  -r1y*iA*r1x-r2y*iB*r2x, mA+r1x^2*iA+mB+r2x^2*iB,           r1x*iA+r2x*iB]
        //     [          -r1y*iA-r2y*iB,           r1x*iA+r2x*iB,                   iA+iB]

        float mA = this._invMassA, mB = this._invMassB;
        float iA = this._invIa, iB = this._invIb;

        Matrix2x2 K = new Matrix2x2();
        K.Ex.X = mA + mB + iA * this._rA.Y * this._rA.Y + iB * this._rB.Y * this._rB.Y;
        K.Ex.Y = -iA * this._rA.X * this._rA.Y - iB * this._rB.X * this._rB.Y;
        K.Ey.X = K.Ex.Y;
        K.Ey.Y = mA + mB + iA * this._rA.X * this._rA.X + iB * this._rB.X * this._rB.X;

        this._linearMass = K.GetInverse();

        this._angularMass = iA + iB;
        if (this._angularMass > 0.0f)
        {
            this._angularMass = 1.0f / this._angularMass;
        }

        if (data.Step.WarmStarting)
        {
            // Scale impulses to support a variable time step.
            this._linearImpulse *= data.Step.DtRatio;
            this._angularImpulse *= data.Step.DtRatio;

            Vector2 P = new Vector2(this._linearImpulse.X, this._linearImpulse.Y);
            vA -= mA * P;
            wA -= iA * (MathUtils.Cross(this._rA, P) + this._angularImpulse);
            vB += mB * P;
            wB += iB * (MathUtils.Cross(this._rB, P) + this._angularImpulse);
        }
        else
        {
            this._linearImpulse.SetZero();
            this._angularImpulse = 0.0f;
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

        float mA = this._invMassA, mB = this._invMassB;
        float iA = this._invIa, iB = this._invIb;

        float h = data.Step.Dt;

        // Solve angular friction
        {
            float Cdot = wB - wA;
            float impulse = -this._angularMass * Cdot;

            float oldImpulse = this._angularImpulse;
            float maxImpulse = h * this._maxTorque;
            this._angularImpulse = MathUtils.Clamp(this._angularImpulse + impulse, -maxImpulse, maxImpulse);
            impulse = this._angularImpulse - oldImpulse;

            wA -= iA * impulse;
            wB += iB * impulse;
        }

        // Solve linear friction
        {
            Vector2 Cdot = vB + MathUtils.Cross(wB, this._rB) - vA - MathUtils.Cross(wA, this._rA);

            Vector2 impulse = -MathUtils.Mul(this._linearMass, Cdot);
            Vector2 oldImpulse = this._linearImpulse;
            this._linearImpulse += impulse;

            float maxImpulse = h * this._maxForce;

            if (this._linearImpulse.LengthSquared() > maxImpulse * maxImpulse)
            {
                this._linearImpulse.Normalize();
                this._linearImpulse *= maxImpulse;
            }

            impulse = this._linearImpulse - oldImpulse;

            vA -= mA * impulse;
            wA -= iA * MathUtils.Cross(this._rA, impulse);

            vB += mB * impulse;
            wB += iB * MathUtils.Cross(this._rB, impulse);
        }

        data.Velocities[this._indexA].V = vA;
        data.Velocities[this._indexA].W = wA;
        data.Velocities[this._indexB].V = vB;
        data.Velocities[this._indexB].W = wB;
    }

    /// <inheritdoc />
    internal override bool SolvePositionConstraints(in SolverData data)
    {
        return true;
    }
}