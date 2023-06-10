namespace ZxenLib.Physics.Dynamics.Joints;

using System.Diagnostics;
using Microsoft.Xna.Framework;
using Common;

/// A motor joint is used to control the relative motion
/// between two bodies. A typical usage is to control the movement
/// of a dynamic body with respect to the ground.
public class MotorJoint : Joint
{
    private float _angularError;

    private float _angularImpulse;

    private float _angularMass;

    private float _angularOffset;

    private float _correctionFactor;

    // Solver temp
    private int _indexA;

    private int _indexB;

    private float _invIa;

    private float _invIb;

    private float _invMassA;

    private float _invMassB;

    private Vector2 _linearError;

    private Vector2 _linearImpulse;

    private Matrix2x2 _linearMass;

    // Solver shared
    private Vector2 _linearOffset;

    private Vector2 _localCenterA;

    private Vector2 _localCenterB;

    private float _maxForce;

    private float _maxTorque;

    private Vector2 _rA;

    private Vector2 _rB;

    internal MotorJoint(MotorJointDef def) : base(def)
    {
        this._linearOffset = def.LinearOffset;
        this._angularOffset = def.AngularOffset;

        this._linearImpulse.SetZero();
        this._angularImpulse = 0.0f;

        this._maxForce = def.MaxForce;
        this._maxTorque = def.MaxTorque;
        this._correctionFactor = def.CorrectionFactor;
    }

    /// Set/get the target linear offset, in frame A, in meters.
    public void SetLinearOffset(in Vector2 linearOffset)
    {
        if (!linearOffset.X.Equals(this._linearOffset.X) || !linearOffset.Y.Equals(this._linearOffset.Y))
        {
            this.BodyA.IsAwake = true;
            this.BodyB.IsAwake = true;
            this._linearOffset = linearOffset;
        }
    }

    public Vector2 GetLinearOffset()
    {
        return this._linearOffset;
    }

    /// Set/get the target angular offset, in radians.
    public void SetAngularOffset(float angularOffset)
    {
        if (!angularOffset.Equals(this._angularOffset))
        {
            this.BodyA.IsAwake = true;
            this.BodyB.IsAwake = true;
            this._angularOffset = angularOffset;
        }
    }

    public float GetAngularOffset()
    {
        return this._angularOffset;
    }

    /// Set the maximum friction force in N.
    public void SetMaxForce(float force)
    {
        Debug.Assert(force.IsValid() && force >= 0.0f);
        this._maxForce = force;
    }

    /// Get the maximum friction force in N.
    public float GetMaxForce()
    {
        return this._maxForce;
    }

    /// Set the maximum friction torque in N*m.
    public void SetMaxTorque(float torque)
    {
        Debug.Assert(torque.IsValid() && torque >= 0.0f);
        this._maxTorque = torque;
    }

    /// Get the maximum friction torque in N*m.
    public float GetMaxTorque()
    {
        return this._maxTorque;
    }

    /// Set the position correction factor in the range [0,1].
    public void SetCorrectionFactor(float factor)
    {
        Debug.Assert(factor.IsValid() && 0.0f <= factor && factor <= 1.0f);
        this._correctionFactor = factor;
    }

    /// Get the position correction factor in the range [0,1].
    public float GetCorrectionFactor()
    {
        return this._correctionFactor;
    }

    /// <inheritdoc />
    public override Vector2 GetAnchorA()
    {
        return this.BodyA.GetPosition();
    }

    /// <inheritdoc />
    public override Vector2 GetAnchorB()
    {
        return this.BodyB.GetPosition();
    }

    /// <inheritdoc />
    public override Vector2 GetReactionForce(float invDt)
    {
        return invDt * this._linearImpulse;
    }

    /// <inheritdoc />
    public override float GetReactionTorque(float invDt)
    {
        return invDt * this._angularImpulse;
    }

    /// Dump to Logger.Log
    public override void Dump()
    {
        int indexA = this.BodyA.IslandIndex;
        int indexB = this.BodyB.IslandIndex;

        DumpLogger.Log("  b2MotorJointDef jd;");
        DumpLogger.Log($"  jd.bodyA = bodies[{indexA}];");
        DumpLogger.Log($"  jd.bodyB = bodies[{indexB}];");
        DumpLogger.Log($"  jd.collideConnected = bool({this.CollideConnected});");
        DumpLogger.Log($"  jd.linearOffset.Set({this._linearOffset.X}, {this._linearOffset.Y});");
        DumpLogger.Log($"  jd.angularOffset = {this._angularOffset};");
        DumpLogger.Log($"  jd.maxForce = {this._maxForce};");
        DumpLogger.Log($"  jd.maxTorque = {this._maxTorque};");
        DumpLogger.Log($"  jd.correctionFactor = {this._correctionFactor};");
        DumpLogger.Log($"  joints[{this.Index}] = m_world.CreateJoint(&jd);");
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

        // Compute the effective mass matrix.
        this._rA = MathUtils.Mul(qA, this._linearOffset - this._localCenterA);
        this._rB = MathUtils.Mul(qB, -this._localCenterB);

        // J = [-I -r1_skew I r2_skew]
        // r_skew = [-ry; rx]

        // Matlab
        // K = [ mA+r1y^2*iA+mB+r2y^2*iB,  -r1y*iA*r1x-r2y*iB*r2x,          -r1y*iA-r2y*iB]
        //     [  -r1y*iA*r1x-r2y*iB*r2x, mA+r1x^2*iA+mB+r2x^2*iB,           r1x*iA+r2x*iB]
        //     [          -r1y*iA-r2y*iB,           r1x*iA+r2x*iB,                   iA+iB]

        float mA = this._invMassA, mB = this._invMassB;
        float iA = this._invIa, iB = this._invIb;

        // Upper 2 by 2 of K for point to point
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

        this._linearError = cB + this._rB - cA - this._rA;
        this._angularError = aB - aA - this._angularOffset;

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
        float invH = data.Step.InvDt;

        // Solve angular friction
        {
            float cdot = wB - wA + invH * this._correctionFactor * this._angularError;
            float impulse = -this._angularMass * cdot;

            float oldImpulse = this._angularImpulse;
            float maxImpulse = h * this._maxTorque;
            this._angularImpulse = MathUtils.Clamp(this._angularImpulse + impulse, -maxImpulse, maxImpulse);
            impulse = this._angularImpulse - oldImpulse;

            wA -= iA * impulse;
            wB += iB * impulse;
        }

        // Solve linear friction
        {
            Vector2 cdot = vB
                           + MathUtils.Cross(wB, this._rB)
                           - vA
                           - MathUtils.Cross(wA, this._rA)
                           + invH * this._correctionFactor * this._linearError;

            Vector2 impulse = -MathUtils.Mul(this._linearMass, cdot);
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