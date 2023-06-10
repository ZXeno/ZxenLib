namespace ZxenLib.Physics.Dynamics.Joints;

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Common;
using Color = Common.Color;

/// <summary>
/// A wheel joint. This joint provides two degrees of freedom: translation
/// along an axis fixed in bodyA and rotation in the plane. In other words, it is a point to
/// line constraint with a rotational motor and a linear spring/damper. The spring/damper is
/// initialized upon creation. This joint is designed for vehicle suspensions.
/// </summary>
public class WheelJoint : Joint
{
    private readonly Vector2 _localAnchorA;

    private readonly Vector2 _localAnchorB;

    private readonly Vector2 _localXAxisA;

    private readonly Vector2 _localYAxisA;

    private float _impulse;

    private float _motorImpulse;

    private float _springImpulse;

    private float _lowerImpulse;

    private float _upperImpulse;

    private float _translation;

    private float _lowerTranslation;

    private float _upperTranslation;

    private float _maxMotorTorque;

    private float _motorSpeed;

    private bool _enableLimit;

    private bool _enableMotor;

    private float _stiffness;

    private float _damping;

    // Solver temp
    private int _indexA;

    private int _indexB;

    private Vector2 _localCenterA;

    private Vector2 _localCenterB;

    private float _invMassA;

    private float _invMassB;

    private float _invIA;

    private float _invIB;

    private Vector2 _ax, _ay;

    private float _sAx, _sBx;

    private float _sAy, _sBy;

    private float _mass;

    private float _motorMass;

    private float _axialMass;

    private float _springMass;

    private float _bias;

    private float _gamma;

    internal WheelJoint(WheelJointDef def)
        : base(def)
    {
        this._localAnchorA = def.LocalAnchorA;
        this._localAnchorB = def.LocalAnchorB;
        this._localXAxisA = def.LocalAxisA;
        this._localYAxisA = MathUtils.Cross(1.0f, this._localXAxisA);

        this._mass = 0.0f;
        this._impulse = 0.0f;
        this._motorMass = 0.0f;
        this._motorImpulse = 0.0f;
        this._springMass = 0.0f;
        this._springImpulse = 0.0f;

        this._axialMass = 0.0f;
        this._lowerImpulse = 0.0f;
        this._upperImpulse = 0.0f;
        this._lowerTranslation = def.LowerTranslation;
        this._upperTranslation = def.UpperTranslation;
        this._enableLimit = def.EnableLimit;

        this._maxMotorTorque = def.MaxMotorTorque;
        this._motorSpeed = def.MotorSpeed;
        this._enableMotor = def.EnableMotor;

        this._bias = 0.0f;
        this._gamma = 0.0f;

        this._ax.SetZero();
        this._ay.SetZero();

        this._stiffness = def.Stiffness;
        this._damping = def.Damping;
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

    /// The local joint axis relative to bodyA.
    public Vector2 GetLocalAxisA()
    {
        return this._localXAxisA;
    }

    /// Get the current joint translation, usually in meters.
    public float GetJointTranslation()
    {
        Body? bA = this.BodyA;
        Body? bB = this.BodyB;

        Vector2 pA = bA.GetWorldPoint(this._localAnchorA);
        Vector2 pB = bB.GetWorldPoint(this._localAnchorB);
        Vector2 d = pB - pA;
        Vector2 axis = bA.GetWorldVector(this._localXAxisA);

        float translation = Vector2.Dot(d, axis);
        return translation;
    }

    /// Get the current joint linear speed, usually in meters per second.
    public float GetJointLinearSpeed()
    {
        Body? bA = this.BodyA;
        Body? bB = this.BodyB;

        Vector2 rA = MathUtils.Mul(bA.Transform.Rotation, this._localAnchorA - bA.Sweep.LocalCenter);
        Vector2 rB = MathUtils.Mul(bB.Transform.Rotation, this._localAnchorB - bB.Sweep.LocalCenter);
        Vector2 p1 = bA.Sweep.C + rA;
        Vector2 p2 = bB.Sweep.C + rB;
        Vector2 d = p2 - p1;
        Vector2 axis = MathUtils.Mul(bA.Transform.Rotation, this._localXAxisA);

        Vector2 vA = bA.LinearVelocity;
        Vector2 vB = bB.LinearVelocity;
        float wA = bA.AngularVelocity;
        float wB = bB.AngularVelocity;

        float speed = Vector2.Dot(d, MathUtils.Cross(wA, axis))
                      + Vector2.Dot(axis, vB + MathUtils.Cross(wB, rB) - vA - MathUtils.Cross(wA, rA));
        return speed;
    }

    /// Get the current joint angle in radians.
    public float GetJointAngle()
    {
        Body? bA = this.BodyA;
        Body? bB = this.BodyB;
        return bB.Sweep.A - bA.Sweep.A;
    }

    /// Get the current joint angular speed in radians per second.
    public float GetJointAngularSpeed()
    {
        float wA = this.BodyA.AngularVelocity;
        float wB = this.BodyB.AngularVelocity;
        return wB - wA;
    }

    /// Is the joint limit enabled?
    public bool IsLimitEnabled() => this._enableLimit;

    /// Enable/disable the joint translation limit.
    public void EnableLimit(bool flag)
    {
        if (flag != this._enableLimit)
        {
            this.BodyA.IsAwake = true;
            this.BodyB.IsAwake = true;
            this._enableLimit = flag;
            this._lowerImpulse = 0.0f;
            this._upperImpulse = 0.0f;
        }
    }

    /// Get the lower joint translation limit, usually in meters.
    public float GetLowerLimit() => this._lowerTranslation;

    /// Get the upper joint translation limit, usually in meters.
    public float GetUpperLimit() => this._upperTranslation;

    /// Set the joint translation limits, usually in meters.
    public void SetLimits(float lower, float upper)
    {
        Debug.Assert(lower <= upper);
        if (!lower.Equals(this._lowerTranslation) || !upper.Equals(this._upperTranslation))
        {
            this.BodyA.IsAwake = true;
            this.BodyB.IsAwake = true;
            this._lowerTranslation = lower;
            this._upperTranslation = upper;
            this._lowerImpulse = 0.0f;
            this._upperImpulse = 0.0f;
        }
    }

    /// Is the joint motor enabled?
    public bool IsMotorEnabled()
    {
        return this._enableMotor;
    }

    /// Enable/disable the joint motor.
    public void EnableMotor(bool flag)
    {
        if (flag != this._enableMotor)
        {
            this.BodyA.IsAwake = true;
            this.BodyB.IsAwake = true;
            this._enableMotor = flag;
        }
    }

    /// Set the motor speed, usually in radians per second.
    public void SetMotorSpeed(float speed)
    {
        if (!speed.Equals(this._motorSpeed))
        {
            this.BodyA.IsAwake = true;
            this.BodyB.IsAwake = true;
            this._motorSpeed = speed;
        }
    }

    /// Get the motor speed, usually in radians per second.
    public float GetMotorSpeed()
    {
        return this._motorSpeed;
    }

    /// Set/Get the maximum motor force, usually in N-m.
    public void SetMaxMotorTorque(float torque)
    {
        if (!torque.Equals(this._maxMotorTorque))
        {
            this.BodyA.IsAwake = true;
            this.BodyB.IsAwake = true;
            this._maxMotorTorque = torque;
        }
    }

    public float GetMaxMotorTorque()
    {
        return this._maxMotorTorque;
    }

    /// Get the current motor torque given the inverse time step, usually in N-m.
    public float GetMotorTorque(float inv_dt)
    {
        return inv_dt * this._motorImpulse;
    }

    /// Access spring stiffness
    public void SetStiffness(float stiffness) => this._stiffness = stiffness;

    public float GetStiffness() => this._stiffness;

    /// Access damping
    public void SetDamping(float damping) => this._damping = damping;

    public float GetDamping() => this._damping;

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
        return inv_dt * (this._impulse * this._ay + (this._springImpulse + this._lowerImpulse - this._upperImpulse) * this._ax);
    }

    /// <inheritdoc />
    public override float GetReactionTorque(float inv_dt)
    {
        return inv_dt * this._motorImpulse;
    }

    /// Dump to Logger.Log
    public override void Dump()
    {
        throw new NotImplementedException();
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
        this._invIA = this.BodyA.InverseInertia;
        this._invIB = this.BodyB.InverseInertia;

        float mA = this._invMassA, mB = this._invMassB;
        float iA = this._invIA, iB = this._invIB;

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

        // Compute the effective masses.
        Vector2 rA = MathUtils.Mul(qA, this._localAnchorA - this._localCenterA);
        Vector2 rB = MathUtils.Mul(qB, this._localAnchorB - this._localCenterB);
        Vector2 d = cB + rB - cA - rA;

        // Point to line constraint
        {
            this._ay = MathUtils.Mul(qA, this._localYAxisA);
            this._sAy = MathUtils.Cross(d + rA, this._ay);
            this._sBy = MathUtils.Cross(rB, this._ay);

            this._mass = mA + mB + iA * this._sAy * this._sAy + iB * this._sBy * this._sBy;

            if (this._mass > 0.0f)
            {
                this._mass = 1.0f / this._mass;
            }
        }

        // Spring constraint
        this._ax = MathUtils.Mul(qA, this._localXAxisA);
        this._sAx = MathUtils.Cross(d + rA, this._ax);
        this._sBx = MathUtils.Cross(rB, this._ax);

        float invMass = mA + mB + iA * this._sAx * this._sAx + iB * this._sBx * this._sBx;
        if (invMass > 0.0f)
        {
            this._axialMass = 1.0f / invMass;
        }
        else
        {
            this._axialMass = 0.0f;
        }

        this._springMass = 0.0f;
        this._bias = 0.0f;
        this._gamma = 0.0f;

        if (this._stiffness > 0.0f && invMass > 0.0f)
        {
            this._springMass = 1.0f / invMass;

            float C = Vector2.Dot(d, this._ax);

            // magic formulas
            float h = data.Step.Dt;
            this._gamma = h * (this._damping + h * this._stiffness);
            if (this._gamma > 0.0f)
            {
                this._gamma = 1.0f / this._gamma;
            }

            this._bias = C * h * this._stiffness * this._gamma;

            this._springMass = invMass + this._gamma;
            if (this._springMass > 0.0f)
            {
                this._springMass = 1.0f / this._springMass;
            }
        }
        else
        {
            this._springImpulse = 0.0f;
        }

        if (this._enableLimit)
        {
            this._translation = Vector2.Dot(this._ax, d);
        }
        else
        {
            this._lowerImpulse = 0.0f;
            this._upperImpulse = 0.0f;
        }

        if (this._enableMotor)
        {
            this._motorMass = iA + iB;
            if (this._motorMass > 0.0f)
            {
                this._motorMass = 1.0f / this._motorMass;
            }
        }
        else
        {
            this._motorMass = 0.0f;
            this._motorImpulse = 0.0f;
        }

        if (data.Step.WarmStarting)
        {
            // Account for variable time step.
            this._impulse *= data.Step.DtRatio;
            this._springImpulse *= data.Step.DtRatio;
            this._motorImpulse *= data.Step.DtRatio;

            float axialImpulse = this._springImpulse + this._lowerImpulse - this._upperImpulse;
            Vector2 P = this._impulse * this._ay + axialImpulse * this._ax;
            float LA = this._impulse * this._sAy + axialImpulse * this._sAx + this._motorImpulse;
            float LB = this._impulse * this._sBy + axialImpulse * this._sBx + this._motorImpulse;

            vA -= this._invMassA * P;
            wA -= this._invIA * LA;

            vB += this._invMassB * P;
            wB += this._invIB * LB;
        }
        else
        {
            this._impulse = 0.0f;
            this._springImpulse = 0.0f;
            this._motorImpulse = 0.0f;
            this._lowerImpulse = 0.0f;
            this._upperImpulse = 0.0f;
        }

        data.Velocities[this._indexA].V = vA;
        data.Velocities[this._indexA].W = wA;
        data.Velocities[this._indexB].V = vB;
        data.Velocities[this._indexB].W = wB;
    }

    /// <inheritdoc />
    internal override void SolveVelocityConstraints(in SolverData data)
    {
        float mA = this._invMassA, mB = this._invMassB;
        float iA = this._invIA, iB = this._invIB;

        Vector2 vA = data.Velocities[this._indexA].V;
        float wA = data.Velocities[this._indexA].W;
        Vector2 vB = data.Velocities[this._indexB].V;
        float wB = data.Velocities[this._indexB].W;

        // Solve spring constraint
        {
            float Cdot = Vector2.Dot(this._ax, vB - vA) + this._sBx * wB - this._sAx * wA;
            float impulse = -this._springMass * (Cdot + this._bias + this._gamma * this._springImpulse);
            this._springImpulse += impulse;

            Vector2 P = impulse * this._ax;
            float LA = impulse * this._sAx;
            float LB = impulse * this._sBx;

            vA -= mA * P;
            wA -= iA * LA;

            vB += mB * P;
            wB += iB * LB;
        }

        // Solve rotational motor constraint
        {
            float Cdot = wB - wA - this._motorSpeed;
            float impulse = -this._motorMass * Cdot;

            float oldImpulse = this._motorImpulse;
            float maxImpulse = data.Step.Dt * this._maxMotorTorque;
            this._motorImpulse = MathUtils.Clamp(this._motorImpulse + impulse, -maxImpulse, maxImpulse);
            impulse = this._motorImpulse - oldImpulse;

            wA -= iA * impulse;
            wB += iB * impulse;
        }
        if (this._enableLimit)
        {
            // Lower limit
            {
                float C = this._translation - this._lowerTranslation;
                float Cdot = Vector2.Dot(this._ax, vB - vA) + this._sBx * wB - this._sAx * wA;
                float impulse = -this._axialMass * (Cdot + Math.Max(C, 0.0f) * data.Step.InvDt);
                float oldImpulse = this._lowerImpulse;
                this._lowerImpulse = Math.Max(this._lowerImpulse + impulse, 0.0f);
                impulse = this._lowerImpulse - oldImpulse;

                Vector2 P = impulse * this._ax;
                float LA = impulse * this._sAx;
                float LB = impulse * this._sBx;

                vA -= mA * P;
                wA -= iA * LA;
                vB += mB * P;
                wB += iB * LB;
            }

            // Upper limit
            // Note: signs are flipped to keep C positive when the constraint is satisfied.
            // This also keeps the impulse positive when the limit is active.
            {
                float C = this._upperTranslation - this._translation;
                float Cdot = Vector2.Dot(this._ax, vA - vB) + this._sAx * wA - this._sBx * wB;
                float impulse = -this._axialMass * (Cdot + Math.Max(C, 0.0f) * data.Step.InvDt);
                float oldImpulse = this._upperImpulse;
                this._upperImpulse = Math.Max(this._upperImpulse + impulse, 0.0f);
                impulse = this._upperImpulse - oldImpulse;

                Vector2 P = impulse * this._ax;
                float LA = impulse * this._sAx;
                float LB = impulse * this._sBx;

                vA += mA * P;
                wA += iA * LA;
                vB -= mB * P;
                wB -= iB * LB;
            }
        }

        // Solve point to line constraint
        {
            float Cdot = Vector2.Dot(this._ay, vB - vA) + this._sBy * wB - this._sAy * wA;
            float impulse = -this._mass * Cdot;
            this._impulse += impulse;

            Vector2 P = impulse * this._ay;
            float LA = impulse * this._sAy;
            float LB = impulse * this._sBy;

            vA -= mA * P;
            wA -= iA * LA;

            vB += mB * P;
            wB += iB * LB;
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

        float linearError = 0.0f;

        if (this._enableLimit)
        {
            Rotation qA = new Rotation(aA);
            Rotation qB = new Rotation(aB);

            Vector2 rA = MathUtils.Mul(qA, this._localAnchorA - this._localCenterA);
            Vector2 rB = MathUtils.Mul(qB, this._localAnchorB - this._localCenterB);
            Vector2 d = (cB - cA) + rB - rA;

            Vector2 ax = MathUtils.Mul(qA, this._localXAxisA);
            float sAx = MathUtils.Cross(d + rA, this._ax);
            float sBx = MathUtils.Cross(rB, this._ax);

            float C = 0.0f;
            float translation = Vector2.Dot(ax, d);
            if (Math.Abs(this._upperTranslation - this._lowerTranslation) < 2.0f * Settings.LinearSlop)
            {
                C = translation;
            }
            else if (translation <= this._lowerTranslation)
            {
                C = Math.Min(translation - this._lowerTranslation, 0.0f);
            }
            else if (translation >= this._upperTranslation)
            {
                C = Math.Max(translation - this._upperTranslation, 0.0f);
            }

            if (!C.Equals(0))
            {
                float invMass = this._invMassA + this._invMassB + this._invIA * sAx * sAx + this._invIB * sBx * sBx;
                float impulse = 0.0f;
                if (!invMass.Equals(0))
                {
                    impulse = -C / invMass;
                }

                Vector2 P = impulse * ax;
                float LA = impulse * sAx;
                float LB = impulse * sBx;

                cA -= this._invMassA * P;
                aA -= this._invIA * LA;
                cB += this._invMassB * P;
                aB += this._invIB * LB;

                linearError = Math.Abs(C);
            }
        }

        // Solve perpendicular constraint
        {
            Rotation qA = new Rotation(aA);
            Rotation qB = new Rotation(aB);

            Vector2 rA = MathUtils.Mul(qA, this._localAnchorA - this._localCenterA);
            Vector2 rB = MathUtils.Mul(qB, this._localAnchorB - this._localCenterB);
            Vector2 d = (cB - cA) + rB - rA;

            Vector2 ay = MathUtils.Mul(qA, this._localYAxisA);

            float sAy = MathUtils.Cross(d + rA, ay);
            float sBy = MathUtils.Cross(rB, ay);

            float C = Vector2.Dot(d, ay);

            float invMass = this._invMassA + this._invMassB + this._invIA * this._sAy * this._sAy + this._invIB * this._sBy * this._sBy;

            float impulse = 0.0f;
            if (!invMass.Equals(0))
            {
                impulse = -C / invMass;
            }

            Vector2 P = impulse * ay;
            float LA = impulse * sAy;
            float LB = impulse * sBy;

            cA -= this._invMassA * P;
            aA -= this._invIA * LA;
            cB += this._invMassB * P;
            aB += this._invIB * LB;

            linearError = Math.Max(linearError, Math.Abs(C));
        }

        data.Positions[this._indexA].Center = cA;
        data.Positions[this._indexA].Angle = aA;
        data.Positions[this._indexB].Center = cB;
        data.Positions[this._indexB].Angle = aB;

        return linearError <= Settings.LinearSlop;
    }

    /// <inheritdoc />
    public override void Draw(IDrawer drawer)
    {
        Transform xfA = this.BodyA.GetTransform();
        Transform xfB = this.BodyB.GetTransform();
        Vector2 pA = MathUtils.Mul(xfA, this._localAnchorA);
        Vector2 pB = MathUtils.Mul(xfB, this._localAnchorB);

        Vector2 axis = MathUtils.Mul(xfA.Rotation, this._localXAxisA);

        Color c1 = Color.FromArgb(0.7f, 0.7f, 0.7f);
        Color c2 = Color.FromArgb(0.3f, 0.9f, 0.3f);
        Color c3 = Color.FromArgb(0.9f, 0.3f, 0.3f);
        Color c4 = Color.FromArgb(0.3f, 0.3f, 0.9f);
        Color c5 = Color.FromArgb(0.4f, 0.4f, 0.4f);

        drawer.DrawSegment(pA, pB, c5);

        if (this._enableLimit)
        {
            Vector2 lower = pA + this._lowerTranslation * axis;
            Vector2 upper = pA + this._upperTranslation * axis;
            Vector2 perp = MathUtils.Mul(xfA.Rotation, this._localYAxisA);
            drawer.DrawSegment(lower, upper, c1);
            drawer.DrawSegment(lower - 0.5f * perp, lower + 0.5f * perp, c2);
            drawer.DrawSegment(upper - 0.5f * perp, upper + 0.5f * perp, c3);
        }
        else
        {
            drawer.DrawSegment(pA - 1.0f * axis, pA + 1.0f * axis, c1);
        }

        drawer.DrawPoint(pA, 5.0f, c1);
        drawer.DrawPoint(pB, 5.0f, c4);
    }
}