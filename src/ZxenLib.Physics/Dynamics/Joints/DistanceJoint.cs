namespace ZxenLib.Physics.Dynamics.Joints;

using System;
using Microsoft.Xna.Framework;
using Common;
using ZxenLib.Physics.Common;
using Color = Common.Color;

/// A distance joint constrains two points on two bodies to remain at a fixed
/// distance from each other. You can view this as a massless, rigid rod.
public class DistanceJoint : Joint
{
    // Solver shared
    private readonly Vector2 _localAnchorA;

    private readonly Vector2 _localAnchorB;

    private float _bias;

    private float _gamma;

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

    private Vector2 _u;

    /// The rest length
    private float _length;

    private float _minLength;

    private float _maxLength;

    private float _currentLength;

    private float _lowerImpulse;

    private float _upperImpulse;

    internal DistanceJoint(DistanceJointDef def)
        : base(def)
    {
        this._localAnchorA = def.LocalAnchorA;
        this._localAnchorB = def.LocalAnchorB;
        this._length = Math.Max(def.Length, Settings.LinearSlop);
        this._minLength = Math.Max(def.MinLength, Settings.LinearSlop);
        this._maxLength = Math.Max(def.MaxLength, this._minLength);
        this.Stiffness = def.Stiffness;
        this.Damping = def.Damping;
        this._impulse = 0.0f;
        this._gamma = 0.0f;
        this._bias = 0.0f;
        this._impulse = 0.0f;
        this._lowerImpulse = 0.0f;
        this._upperImpulse = 0.0f;
        this._currentLength = 0.0f;
    }

    /// Set/get the linear stiffness in N/m
    public float Stiffness { get; set; }

    /// Set/get linear damping in N*s/m
    public float Damping { get; set; }

    public float SoftMass { get; set; }

    public override Vector2 GetAnchorA()
    {
        return this.BodyA.GetWorldPoint(this._localAnchorA);
    }

    public override Vector2 GetAnchorB()
    {
        return this.BodyB.GetWorldPoint(this._localAnchorB);
    }

    /// Get the reaction force given the inverse time step.
    /// Unit is N.
    public override Vector2 GetReactionForce(float inv_dt)
    {
        Vector2 F = inv_dt * (this._impulse + this._lowerImpulse - this._upperImpulse) * this._u;
        return F;
    }

    /// Get the reaction torque given the inverse time step.
    /// Unit is N*m. This is always zero for a distance joint.
    public override float GetReactionTorque(float inv_dt)
    {
        return 0.0f;
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

    public float SetLength(float length)
    {
        this._impulse = 0.0f;
        this._length = Math.Max(Settings.LinearSlop, length);
        return this._length;
    }

    public float SetMinLength(float minLength)
    {
        this._lowerImpulse = 0.0f;
        this._minLength = MathUtils.Clamp(minLength, Settings.LinearSlop, this._maxLength);
        return this._minLength;
    }

    public float SetMaxLength(float maxLength)
    {
        this._upperImpulse = 0.0f;
        this._maxLength = Math.Max(maxLength, this._minLength);
        return this._maxLength;
    }

    public float GetCurrentLength()
    {
        Vector2 pA = this.BodyA.GetWorldPoint(this._localAnchorA);
        Vector2 pB = this.BodyB.GetWorldPoint(this._localAnchorB);
        Vector2 d = pB - pA;
        float length = d.Length();
        return length;
    }

    /// Dump joint to dmLog
    public override void Dump()
    {
        // Todo
    }

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
        this._u = cB + this._rB - cA - this._rA;

        // Handle singularity.
        this._currentLength = this._u.Length();
        if (this._currentLength > Settings.LinearSlop)
        {
            this._u *= 1.0f / this._currentLength;
        }
        else
        {
            this._u.Set(0.0f, 0.0f);
            this._mass = 0.0f;
            this._impulse = 0.0f;
            this._lowerImpulse = 0.0f;
            this._upperImpulse = 0.0f;
        }

        float crAu = MathUtils.Cross(this._rA, this._u);
        float crBu = MathUtils.Cross(this._rB, this._u);
        float invMass = this._invMassA + this._invIa * crAu * crAu + this._invMassB + this._invIb * crBu * crBu;
        this._mass = invMass != 0.0f ? 1.0f / invMass : 0.0f;
        if (this.Stiffness > 0.0f && this._minLength < this._maxLength)
        {
            // soft
            float C = this._currentLength - this._length;

            float d = this.Damping;
            float k = this.Stiffness;

            // magic formulas
            float h = data.Step.Dt;

            // gamma = 1 / (h * (d + h * k))
            // the extra factor of h in the denominator is since the lambda is an impulse, not a force
            this._gamma = h * (d + h * k);
            this._gamma = !this._gamma.Equals(0.0f) ? 1.0f / this._gamma : 0.0f;
            this._bias = C * h * k * this._gamma;

            invMass += this._gamma;
            this.SoftMass = Math.Abs(invMass) > Settings.Epsilon ? 1.0f / invMass : 0.0f;
        }
        else
        {
            // rigid
            this._gamma = 0.0f;
            this._bias = 0.0f;
            this._mass = invMass != 0.0f ? 1.0f / invMass : 0.0f;
            this.SoftMass = this._mass;
        }

        if (data.Step.WarmStarting)
        {
            // Scale the impulse to support a variable time step.
            this._impulse *= data.Step.DtRatio;
            this._lowerImpulse *= data.Step.DtRatio;
            this._upperImpulse *= data.Step.DtRatio;

            Vector2 P = (this._impulse + this._lowerImpulse - this._upperImpulse) * this._u;
            vA -= this._invMassA * P;
            wA -= this._invIa * MathUtils.Cross(this._rA, P);
            vB += this._invMassB * P;
            wB += this._invIb * MathUtils.Cross(this._rB, P);
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

    internal override void SolveVelocityConstraints(in SolverData data)
    {
        Vector2 vA = data.Velocities[this._indexA].V;
        float wA = data.Velocities[this._indexA].W;
        Vector2 vB = data.Velocities[this._indexB].V;
        float wB = data.Velocities[this._indexB].W;
        if (this._minLength < this._maxLength)
        {
            if (this.Stiffness > 0.0f)
            {
                // Cdot = dot(u, v + cross(w, r))
                Vector2 vpA = vA + MathUtils.Cross(wA, this._rA);
                Vector2 vpB = vB + MathUtils.Cross(wB, this._rB);
                float Cdot = Vector2.Dot(this._u, vpB - vpA);

                float impulse = -this.SoftMass * (Cdot + this._bias + this._gamma * this._impulse);
                this._impulse += impulse;

                Vector2 P = impulse * this._u;
                vA -= this._invMassA * P;
                wA -= this._invIa * MathUtils.Cross(this._rA, P);
                vB += this._invMassB * P;
                wB += this._invIb * MathUtils.Cross(this._rB, P);
            }

            // lower
            {
                float C = this._currentLength - this._minLength;
                float bias = Math.Max(0.0f, C) * data.Step.InvDt;

                Vector2 vpA = vA + MathUtils.Cross(wA, this._rA);
                Vector2 vpB = vB + MathUtils.Cross(wB, this._rB);
                float Cdot = Vector2.Dot(this._u, vpB - vpA);

                float impulse = -this._mass * (Cdot + bias);
                float oldImpulse = this._lowerImpulse;
                this._lowerImpulse = Math.Max(0.0f, this._lowerImpulse + impulse);
                impulse = this._lowerImpulse - oldImpulse;
                Vector2 P = impulse * this._u;

                vA -= this._invMassA * P;
                wA -= this._invIa * MathUtils.Cross(this._rA, P);
                vB += this._invMassB * P;
                wB += this._invIb * MathUtils.Cross(this._rB, P);
            }

            // upper
            {
                float C = this._maxLength - this._currentLength;
                float bias = Math.Max(0.0f, C) * data.Step.InvDt;

                Vector2 vpA = vA + MathUtils.Cross(wA, this._rA);
                Vector2 vpB = vB + MathUtils.Cross(wB, this._rB);
                float Cdot = Vector2.Dot(this._u, vpA - vpB);

                float impulse = -this._mass * (Cdot + bias);
                float oldImpulse = this._upperImpulse;
                this._upperImpulse = Math.Max(0.0f, this._upperImpulse + impulse);
                impulse = this._upperImpulse - oldImpulse;
                Vector2 P = -impulse * this._u;

                vA -= this._invMassA * P;
                wA -= this._invIa * MathUtils.Cross(this._rA, P);
                vB += this._invMassB * P;
                wB += this._invIb * MathUtils.Cross(this._rB, P);
            }
        }
        else
        {
            // Equal limits

            // Cdot = dot(u, v + cross(w, r))
            Vector2 vpA = vA + MathUtils.Cross(wA, this._rA);
            Vector2 vpB = vB + MathUtils.Cross(wB, this._rB);
            float Cdot = Vector2.Dot(this._u, vpB - vpA);

            float impulse = -this._mass * Cdot;
            this._impulse += impulse;

            Vector2 P = impulse * this._u;
            vA -= this._invMassA * P;
            wA -= this._invIa * MathUtils.Cross(this._rA, P);
            vB += this._invMassB * P;
            wB += this._invIb * MathUtils.Cross(this._rB, P);
        }

        data.Velocities[this._indexA].V = vA;
        data.Velocities[this._indexA].W = wA;
        data.Velocities[this._indexB].V = vB;
        data.Velocities[this._indexB].W = wB;
    }

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
        Vector2 u = cB + rB - cA - rA;
        System.Numerics.Vector2 z = new();

        float length =  MathExtensions.Normalize(ref u);
        float C;
        if (Math.Abs(this._minLength - this._maxLength) < Settings.Epsilon)
        {
            C = length - this._minLength;
        }
        else if (length < this._minLength)
        {
            C = length - this._minLength;
        }
        else if (this._maxLength < length)
        {
            C = length - this._maxLength;
        }
        else
        {
            return true;
        }

        float impulse = -this._mass * C;
        Vector2 P = impulse * u;

        cA -= this._invMassA * P;
        aA -= this._invIa * MathUtils.Cross(rA, P);
        cB += this._invMassB * P;
        aB += this._invIb * MathUtils.Cross(rB, P);

        data.Positions[this._indexA].Center = cA;
        data.Positions[this._indexA].Angle = aA;
        data.Positions[this._indexB].Center = cB;
        data.Positions[this._indexB].Angle = aB;

        return Math.Abs(C) < Settings.LinearSlop;
    }

    /// <inheritdoc />
    public override void Draw(IDrawer drawer)
    {
        Transform xfA = this.BodyA.GetTransform();
        Transform xfB = this.BodyB.GetTransform();
        Vector2 pA = MathUtils.Mul(xfA, this._localAnchorA);
        Vector2 pB = MathUtils.Mul(xfB, this._localAnchorB);

        Vector2 axis = pB - pA;
        axis.Normalize();

        Color c1 = Color.FromArgb(0.7f, 0.7f, 0.7f);
        Color c2 = Color.FromArgb(0.3f, 0.9f, 0.3f);
        Color c3 = Color.FromArgb(0.9f, 0.3f, 0.3f);
        Color c4 = Color.FromArgb(0.4f, 0.4f, 0.4f);

        drawer.DrawSegment(pA, pB, c4);

        Vector2 pRest = pA + this._length * axis;
        drawer.DrawPoint(pRest, 8.0f, c1);

        if (Math.Abs(this._minLength - this._maxLength) > Settings.Epsilon)
        {
            if (this._minLength > Settings.LinearSlop)
            {
                Vector2 pMin = pA + this._minLength * axis;
                drawer.DrawPoint(pMin, 4.0f, c2);
            }

            if (this._maxLength < Settings.MaxFloat)
            {
                Vector2 pMax = pA + this._maxLength * axis;
                drawer.DrawPoint(pMax, 4.0f, c3);
            }
        }
    }
}