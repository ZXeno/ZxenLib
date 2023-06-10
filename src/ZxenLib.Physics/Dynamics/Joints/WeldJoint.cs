namespace ZxenLib.Physics.Dynamics.Joints;

using System;
using Microsoft.Xna.Framework;
using Common;

/// A weld joint essentially glues two bodies together. A weld joint may
/// distort somewhat because the island constraint solver is approximate.
public class WeldJoint : Joint
{
    // Solver shared
    private readonly Vector2 _localAnchorA;

    private readonly Vector2 _localAnchorB;

    private readonly float _referenceAngle;

    private float _bias;

    public float Stiffness = 0.0f;

    public float Damping = 0.0f;

    private float _gamma;

    private Vector3 _impulse;

    // Solver temp
    private int _indexA;

    private int _indexB;

    private float _invIa;

    private float _invIb;

    private float _invMassA;

    private float _invMassB;

    private Vector2 _localCenterA;

    private Vector2 _localCenterB;

    private Matrix3x3 _mass;

    private Vector2 _rA;

    private Vector2 _rB;

    internal WeldJoint(WeldJointDef def)
        : base(def)
    {
        this._localAnchorA = def.LocalAnchorA;
        this._localAnchorB = def.LocalAnchorB;
        this._referenceAngle = def.ReferenceAngle;
        this.Damping = def.Damping;
        this.Stiffness = def.Stiffness;

        this._impulse.SetZero();
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

    /// Get the reference angle.
    public float GetReferenceAngle()
    {
        return this._referenceAngle;
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
        Vector2 P = new Vector2(this._impulse.X, this._impulse.Y);
        return inv_dt * P;
    }

    /// <inheritdoc />
    public override float GetReactionTorque(float inv_dt)
    {
        return inv_dt * this._impulse.Z;
    }

    /// Dump to Logger.Log
    public override void Dump()
    {
        // Todo
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

        float aA = data.Positions[this._indexA].Angle;
        Vector2 vA = data.Velocities[this._indexA].V;
        float wA = data.Velocities[this._indexA].W;

        float aB = data.Positions[this._indexB].Angle;
        Vector2 vB = data.Velocities[this._indexB].V;
        float wB = data.Velocities[this._indexB].W;

        Rotation qA = new Rotation(aA);
        Rotation qB = new Rotation(aB);

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

        Matrix3x3 K = new Matrix3x3();
        K.Ex.X = mA + mB + this._rA.Y * this._rA.Y * iA + this._rB.Y * this._rB.Y * iB;
        K.Ey.X = -this._rA.Y * this._rA.X * iA - this._rB.Y * this._rB.X * iB;
        K.Ez.X = -this._rA.Y * iA - this._rB.Y * iB;
        K.Ex.Y = K.Ey.X;
        K.Ey.Y = mA + mB + this._rA.X * this._rA.X * iA + this._rB.X * this._rB.X * iB;
        K.Ez.Y = this._rA.X * iA + this._rB.X * iB;
        K.Ex.Z = K.Ez.X;
        K.Ey.Z = K.Ez.Y;
        K.Ez.Z = iA + iB;

        if (this.Stiffness > 0.0f)
        {
            K.GetInverse22(ref this._mass);

            float invM = iA + iB;

            float C = aB - aA - this._referenceAngle;

            // Damping coefficient
            float d = this.Damping;

            // Spring stiffness
            float k = this.Stiffness;

            // magic formulas
            float h = data.Step.Dt;
            this._gamma = h * (d + h * k);
            this._gamma = !this._gamma.Equals(0.0f) ? 1.0f / this._gamma : 0.0f;
            this._bias = C * h * k * this._gamma;

            invM += this._gamma;
            this._mass.Ez.Z = !invM.Equals(0.0f) ? 1.0f / invM : 0.0f;
        }
        else if (K.Ez.Z.Equals(0.0f))
        {
            K.GetInverse22(ref this._mass);
            this._gamma = 0.0f;
            this._bias = 0.0f;
        }
        else
        {
            K.GetSymInverse33(ref this._mass);
            this._gamma = 0.0f;
            this._bias = 0.0f;
        }

        if (data.Step.WarmStarting)
        {
            // Scale impulses to support a variable time step.
            this._impulse *= data.Step.DtRatio;

            Vector2 P = new Vector2(this._impulse.X, this._impulse.Y);

            vA -= mA * P;
            wA -= iA * (MathUtils.Cross(this._rA, P) + this._impulse.Z);

            vB += mB * P;
            wB += iB * (MathUtils.Cross(this._rB, P) + this._impulse.Z);
        }
        else
        {
            this._impulse.SetZero();
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

        if (this.Stiffness > 0.0f)
        {
            float Cdot2 = wB - wA;

            float impulse2 = -this._mass.Ez.Z * (Cdot2 + this._bias + this._gamma * this._impulse.Z);
            this._impulse.Z += impulse2;

            wA -= iA * impulse2;
            wB += iB * impulse2;

            Vector2 Cdot1 = vB + MathUtils.Cross(wB, this._rB) - vA - MathUtils.Cross(wA, this._rA);

            Vector2 impulse1 = -MathUtils.Mul22(this._mass, Cdot1);
            this._impulse.X += impulse1.X;
            this._impulse.Y += impulse1.Y;

            Vector2 P = impulse1;

            vA -= mA * P;
            wA -= iA * MathUtils.Cross(this._rA, P);

            vB += mB * P;
            wB += iB * MathUtils.Cross(this._rB, P);
        }
        else
        {
            Vector2 cdot1 = vB + MathUtils.Cross(wB, this._rB) - vA - MathUtils.Cross(wA, this._rA);
            float cdot2 = wB - wA;
            Vector3 cdot = new Vector3(cdot1.X, cdot1.Y, cdot2);

            Vector3 impulse = -MathUtils.Mul(this._mass, cdot);
            this._impulse += impulse;

            Vector2 P = new Vector2(impulse.X, impulse.Y);

            vA -= mA * P;
            wA -= iA * (MathUtils.Cross(this._rA, P) + impulse.Z);

            vB += mB * P;
            wB += iB * (MathUtils.Cross(this._rB, P) + impulse.Z);
        }

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

        float mA = this._invMassA, mB = this._invMassB;
        float iA = this._invIa, iB = this._invIb;

        Vector2 rA = MathUtils.Mul(qA, this._localAnchorA - this._localCenterA);
        Vector2 rB = MathUtils.Mul(qB, this._localAnchorB - this._localCenterB);

        float positionError, angularError;

        Matrix3x3 K = new Matrix3x3();
        K.Ex.X = mA + mB + rA.Y * rA.Y * iA + rB.Y * rB.Y * iB;
        K.Ey.X = -rA.Y * rA.X * iA - rB.Y * rB.X * iB;
        K.Ez.X = -rA.Y * iA - rB.Y * iB;
        K.Ex.Y = K.Ey.X;
        K.Ey.Y = mA + mB + rA.X * rA.X * iA + rB.X * rB.X * iB;
        K.Ez.Y = rA.X * iA + rB.X * iB;
        K.Ex.Z = K.Ez.X;
        K.Ey.Z = K.Ez.Y;
        K.Ez.Z = iA + iB;

        if (this.Stiffness > 0.0f)
        {
            Vector2 C1 = cB + rB - cA - rA;

            positionError = C1.Length();
            angularError = 0.0f;

            Vector2 P = -K.Solve22(C1);

            cA -= mA * P;
            aA -= iA * MathUtils.Cross(rA, P);

            cB += mB * P;
            aB += iB * MathUtils.Cross(rB, P);
        }
        else
        {
            Vector2 C1 = cB + rB - cA - rA;
            float C2 = aB - aA - this._referenceAngle;

            positionError = C1.Length();
            angularError = Math.Abs(C2);

            Vector3 C = new Vector3(C1.X, C1.Y, C2);

            Vector3 impulse = new Vector3();
            if (K.Ez.Z > 0.0f)
            {
                impulse = -K.Solve33(C);
            }
            else
            {
                Vector2 impulse2 = -K.Solve22(C1);
                impulse.Set(impulse2.X, impulse2.Y, 0.0f);
            }

            Vector2 P = new Vector2(impulse.X, impulse.Y);

            cA -= mA * P;
            aA -= iA * (MathUtils.Cross(rA, P) + impulse.Z);

            cB += mB * P;
            aB += iB * (MathUtils.Cross(rB, P) + impulse.Z);
        }

        data.Positions[this._indexA].Center = cA;
        data.Positions[this._indexA].Angle = aA;
        data.Positions[this._indexB].Center = cB;
        data.Positions[this._indexB].Angle = aB;

        return positionError <= Settings.LinearSlop && angularError <= Settings.AngularSlop;
    }
}