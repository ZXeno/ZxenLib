namespace ZxenLib.Physics.Dynamics.Joints;

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Common;
using Color = Common.Color;

/// <summary>
/// Linear constraint (point-to-line)
/// d = p2 - p1 = x2 + r2 - x1 - r1
/// C = dot(perp, d)
/// Cdot = dot(d, cross(w1, perp)) + dot(perp, v2 + cross(w2, r2) - v1 - cross(w1, r1))
///      = -dot(perp, v1) - dot(cross(d + r1, perp), w1) + dot(perp, v2) + dot(cross(r2, perp), v2)
/// J = [-perp, -cross(d + r1, perp), perp, cross(r2,perp)]
///
/// Angular constraint
/// C = a2 - a1 + a_initial
/// Cdot = w2 - w1
/// J = [0 0 -1 0 0 1]
///
/// K = J * invM * JT
///
/// J = [-a -s1 a s2]
///     [0  -1  0  1]
/// a = perp
/// s1 = cross(d + r1, a) = cross(p2 - x1, a)
/// s2 = cross(r2, a) = cross(p2 - x2, a)
///
/// Motor/Limit linear constraint
/// C = dot(ax1, d)
/// Cdot = -dot(ax1, v1) - dot(cross(d + r1, ax1), w1) + dot(ax1, v2) + dot(cross(r2, ax1), v2)
/// J = [-ax1 -cross(d+r1,ax1) ax1 cross(r2,ax1)]
///
/// Predictive limit is applied even when the limit is not active.
/// Prevents a constraint speed that can lead to a constraint error in one time step.
/// Want C2 = C1 + h * Cdot >= 0
/// Or:
/// Cdot + C1/h >= 0
/// I do not apply a negative constraint error because that is handled in position correction.
/// So:
/// Cdot + max(C1, 0)/h >= 0
///
/// Block Solver
/// We develop a block solver that includes the angular and linear constraints. This makes the limit stiffer.
///
/// The Jacobian has 2 rows:
/// J = [-uT -s1 uT s2] /// linear
///     [0   -1   0  1] /// angular
///
/// u = perp
/// s1 = cross(d + r1, u), s2 = cross(r2, u)
/// a1 = cross(d + r1, v), a2 = cross(r2, v)
/// </summary>
public class PrismaticJoint : Joint
{
    internal readonly Vector2 LocalAnchorA;

    internal readonly Vector2 LocalAnchorB;

    internal readonly Vector2 LocalXAxisA;

    internal readonly Vector2 LocalYAxisA;

    internal readonly float ReferenceAngle;

    private Vector2 _impulse;

    private float _motorImpulse;

    private float _lowerImpulse;

    private float _upperImpulse;

    private float _lowerTranslation;

    private float _upperTranslation;

    private float _maxMotorForce;

    private float _motorSpeed;

    private bool _enableLimit;

    private bool _enableMotor;

    #region Solver temp

    private int _indexA;

    private int _indexB;

    private Vector2 _localCenterA;

    private Vector2 _localCenterB;

    private float _invMassA;

    private float _invMassB;

    private float _invIA;

    private float _invIB;

    private Vector2 _axis, _perp;

    private float _s1, _s2;

    private float _a1, _a2;

    private Matrix2x2 _k;

    private float _translation;

    private float _axialMass;

    #endregion

    internal PrismaticJoint(PrismaticJointDef def)
        : base(def)
    {
        this.LocalAnchorA = def.LocalAnchorA;
        this.LocalAnchorB = def.LocalAnchorB;
        this.LocalXAxisA = def.LocalAxisA;
        this.LocalXAxisA.Normalize();
        this.LocalYAxisA = MathUtils.Cross(1.0f, this.LocalXAxisA);
        this.ReferenceAngle = def.ReferenceAngle;

        this._impulse.SetZero();
        this._axialMass = 0.0f;
        this._motorImpulse = 0.0f;
        this._lowerImpulse = 0.0f;
        this._upperImpulse = 0.0f;

        this._lowerTranslation = def.LowerTranslation;
        this._upperTranslation = def.UpperTranslation;

        Debug.Assert(this._lowerTranslation <= this._upperTranslation);

        this._maxMotorForce = def.MaxMotorForce;
        this._motorSpeed = def.MotorSpeed;
        this._enableLimit = def.EnableLimit;
        this._enableMotor = def.EnableMotor;

        this._translation = 0.0f;
        this._axis.SetZero();
        this._perp.SetZero();
    }

    /// The local anchor point relative to bodyA's origin.
    public Vector2 GetLocalAnchorA()
    {
        return this.LocalAnchorA;
    }

    /// The local anchor point relative to bodyB's origin.
    public Vector2 GetLocalAnchorB()
    {
        return this.LocalAnchorB;
    }

    /// The local joint axis relative to bodyA.
    public Vector2 GetLocalAxisA()
    {
        return this.LocalXAxisA;
    }

    /// Get the reference angle.
    public float GetReferenceAngle()
    {
        return this.ReferenceAngle;
    }

    /// Get the current joint translation, usually in meters.
    public float GetJointTranslation()
    {
        Vector2 pA = this.BodyA.GetWorldPoint(this.LocalAnchorA);
        Vector2 pB = this.BodyB.GetWorldPoint(this.LocalAnchorB);
        Vector2 d = pB - pA;
        Vector2 axis = this.BodyA.GetWorldVector(this.LocalXAxisA);

        float translation = Vector2.Dot(d, axis);
        return translation;
    }

    /// Get the current joint translation speed, usually in meters per second.
    public float GetJointSpeed()
    {
        Body? bA = this.BodyA;
        Body? bB = this.BodyB;

        Vector2 rA = MathUtils.Mul(bA.Transform.Rotation, this.LocalAnchorA - bA.Sweep.LocalCenter);
        Vector2 rB = MathUtils.Mul(bB.Transform.Rotation, this.LocalAnchorB - bB.Sweep.LocalCenter);
        Vector2 p1 = bA.Sweep.C + rA;
        Vector2 p2 = bB.Sweep.C + rB;
        Vector2 d = p2 - p1;
        Vector2 axis = MathUtils.Mul(bA.Transform.Rotation, this.LocalXAxisA);

        Vector2 vA = bA.LinearVelocity;
        Vector2 vB = bB.LinearVelocity;
        float wA = bA.AngularVelocity;
        float wB = bB.AngularVelocity;

        float speed = Vector2.Dot(d, MathUtils.Cross(wA, axis))
                      + Vector2.Dot(axis, vB + MathUtils.Cross(wB, rB) - vA - MathUtils.Cross(wA, rA));
        return speed;
    }

    /// Is the joint limit enabled?
    public bool IsLimitEnabled()
    {
        return this._enableLimit;
    }

    /// Enable/disable the joint limit.
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

    /// Get the lower joint limit, usually in meters.
    public float GetLowerLimit()
    {
        return this._lowerTranslation;
    }

    /// Get the upper joint limit, usually in meters.
    public float GetUpperLimit()
    {
        return this._upperTranslation;
    }

    /// Set the joint limits, usually in meters.
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

    /// Set the motor speed, usually in meters per second.
    public void SetMotorSpeed(float speed)
    {
        if (speed != this._motorSpeed)
        {
            this.BodyA.IsAwake = true;
            this.BodyB.IsAwake = true;
            this._motorSpeed = speed;
        }
    }

    /// Get the motor speed, usually in meters per second.
    public float GetMotorSpeed()
    {
        return this._motorSpeed;
    }

    /// Set the maximum motor force, usually in N.
    public void SetMaxMotorForce(float force)
    {
        if (Math.Abs(force - this._maxMotorForce) > 0.000001f)
        {
            this.BodyA.IsAwake = true;
            this.BodyB.IsAwake = true;
            this._maxMotorForce = force;
        }
    }

    public float GetMaxMotorForce()
    {
        return this._maxMotorForce;
    }

    /// Get the current motor force given the inverse time step, usually in N.
    public float GetMotorForce(float inv_dt)
    {
        return inv_dt * this._motorImpulse;
    }

    /// <inheritdoc />
    public override Vector2 GetAnchorA()
    {
        return this.BodyA.GetWorldPoint(this.LocalAnchorA);
    }

    /// <inheritdoc />
    public override Vector2 GetAnchorB()
    {
        return this.BodyB.GetWorldPoint(this.LocalAnchorB);
    }

    /// <inheritdoc />
    public override Vector2 GetReactionForce(float inv_dt)
    {
        return inv_dt * (this._impulse.X * this._perp + (this._motorImpulse + this._lowerImpulse - this._upperImpulse) * this._axis);
    }

    /// <inheritdoc />
    public override float GetReactionTorque(float inv_dt)
    {
        return inv_dt * this._impulse.Y;
    }

    /// Dump to b2Log
    public override void Dump()
    { }

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
        Vector2 rA = MathUtils.Mul(qA, this.LocalAnchorA - this._localCenterA);
        Vector2 rB = MathUtils.Mul(qB, this.LocalAnchorB - this._localCenterB);
        Vector2 d = cB - cA + rB - rA;

        float mA = this._invMassA, mB = this._invMassB;
        float iA = this._invIA, iB = this._invIB;

        // Compute motor Jacobian and effective mass.
        {
            this._axis = MathUtils.Mul(qA, this.LocalXAxisA);
            this._a1 = MathUtils.Cross(d + rA, this._axis);
            this._a2 = MathUtils.Cross(rB, this._axis);

            this._axialMass = mA + mB + iA * this._a1 * this._a1 + iB * this._a2 * this._a2;
            if (this._axialMass > 0.0f)
            {
                this._axialMass = 1.0f / this._axialMass;
            }
        }

        // Prismatic constraint.
        {
            this._perp = MathUtils.Mul(qA, this.LocalYAxisA);

            this._s1 = MathUtils.Cross(d + rA, this._perp);
            this._s2 = MathUtils.Cross(rB, this._perp);

            float k11 = mA + mB + iA * this._s1 * this._s1 + iB * this._s2 * this._s2;
            float k12 = iA * this._s1 + iB * this._s2;
            float k22 = iA + iB;
            if (k22.Equals(0.0f))
            {
                // For bodies with fixed rotation.
                k22 = 1.0f;
            }

            this._k.Ex.Set(k11, k12);
            this._k.Ey.Set(k12, k22);
        }

        if (this._enableLimit)
        {
            this._translation = Vector2.Dot(this._axis, d);
        }
        else
        {
            this._lowerImpulse = 0.0f;
            this._upperImpulse = 0.0f;
        }

        if (this._enableMotor == false)
        {
            this._motorImpulse = 0.0f;
        }

        if (data.Step.WarmStarting)
        {
            // Account for variable time step.
            this._impulse *= data.Step.DtRatio;
            this._motorImpulse *= data.Step.DtRatio;
            this._lowerImpulse = data.Step.DtRatio;
            this._upperImpulse = data.Step.DtRatio;

            float axialImpulse = this._motorImpulse + this._lowerImpulse - this._upperImpulse;
            Vector2 P = this._impulse.X * this._perp + axialImpulse * this._axis;
            float LA = this._impulse.X * this._s1 + this._impulse.Y + axialImpulse * this._a1;
            float LB = this._impulse.X * this._s2 + this._impulse.Y + axialImpulse * this._a2;

            vA -= mA * P;
            wA -= iA * LA;

            vB += mB * P;
            wB += iB * LB;
        }
        else
        {
            this._impulse.SetZero();
            this._motorImpulse = 0.0f;
            this._lowerImpulse = 0.0f;
            this._upperImpulse = 0.0f;
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

        float mA = this._invMassA, mB = this._invMassB;
        float iA = this._invIA, iB = this._invIB;

        // Solve linear motor constraint.
        if (this._enableMotor)
        {
            float Cdot = Vector2.Dot(this._axis, vB - vA) + this._a2 * wB - this._a1 * wA;
            float impulse = this._axialMass * (this._motorSpeed - Cdot);
            float oldImpulse = this._motorImpulse;
            float maxImpulse = data.Step.Dt * this._maxMotorForce;
            this._motorImpulse = MathUtils.Clamp(this._motorImpulse + impulse, -maxImpulse, maxImpulse);
            impulse = this._motorImpulse - oldImpulse;

            Vector2 P = impulse * this._axis;
            float LA = impulse * this._a1;
            float LB = impulse * this._a2;

            vA -= mA * P;
            wA -= iA * LA;

            vB += mB * P;
            wB += iB * LB;
        }

        Vector2 Cdot1;
        Cdot1.X = Vector2.Dot(this._perp, vB - vA) + this._s2 * wB - this._s1 * wA;
        Cdot1.Y = wB - wA;

        if (this._enableLimit)
        {
            // Lower limit
            {
                float C = this._translation - this._lowerTranslation;
                float Cdot = Vector2.Dot(this._axis, vB - vA) + this._a2 * wB - this._a1 * wA;
                float impulse = -this._axialMass * (Cdot + Math.Max(C, 0.0f) * data.Step.InvDt);
                float oldImpulse = this._lowerImpulse;
                this._lowerImpulse = Math.Max(this._lowerImpulse + impulse, 0.0f);
                impulse = this._lowerImpulse - oldImpulse;

                Vector2 P = impulse * this._axis;
                float LA = impulse * this._a1;
                float LB = impulse * this._a2;

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
                float Cdot = Vector2.Dot(this._axis, vA - vB) + this._a1 * wA - this._a2 * wB;
                float impulse = -this._axialMass * (Cdot + Math.Max(C, 0.0f) * data.Step.InvDt);
                float oldImpulse = this._upperImpulse;
                this._upperImpulse = Math.Max(this._upperImpulse + impulse, 0.0f);
                impulse = this._upperImpulse - oldImpulse;

                Vector2 P = impulse * this._axis;
                float LA = impulse * this._a1;
                float LB = impulse * this._a2;

                vA += mA * P;
                wA += iA * LA;
                vB -= mB * P;
                wB -= iB * LB;
            }
        }

        // Solve the prismatic constraint in block form.
        {
            Vector2 Cdot = new Vector2
            {
                X = Vector2.Dot(this._perp, vB - vA) + this._s2 * wB - this._s1 * wA,
                Y = wB - wA
            };

            Vector2 df = this._k.Solve(-Cdot);
            this._impulse += df;

            Vector2 P = df.X * this._perp;
            float LA = df.X * this._s1 + df.Y;
            float LB = df.X * this._s2 + df.Y;

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

    // A velocity based solver computes reaction forces(impulses) using the velocity constraint solver.Under this context,
    // the position solver is not there to resolve forces.It is only there to cope with integration error.
    //
    // Therefore, the pseudo impulses in the position solver do not have any physical meaning.Thus it is okay if they suck.
    //
    // We could take the active state from the velocity solver.However, the joint might push past the limit when the velocity
    // solver indicates the limit is inactive.
    internal override bool SolvePositionConstraints(in SolverData data)
    {
        Vector2 cA = data.Positions[this._indexA].Center;
        float aA = data.Positions[this._indexA].Angle;
        Vector2 cB = data.Positions[this._indexB].Center;
        float aB = data.Positions[this._indexB].Angle;

        Rotation qA = new Rotation(aA);
        Rotation qB = new Rotation(aB);

        float mA = this._invMassA, mB = this._invMassB;
        float iA = this._invIA, iB = this._invIB;

        // Compute fresh Jacobians
        Vector2 rA = MathUtils.Mul(qA, this.LocalAnchorA - this._localCenterA);
        Vector2 rB = MathUtils.Mul(qB, this.LocalAnchorB - this._localCenterB);
        Vector2 d = cB + rB - cA - rA;

        Vector2 axis = MathUtils.Mul(qA, this.LocalXAxisA);
        float a1 = MathUtils.Cross(d + rA, axis);
        float a2 = MathUtils.Cross(rB, axis);
        Vector2 perp = MathUtils.Mul(qA, this.LocalYAxisA);

        float s1 = MathUtils.Cross(d + rA, perp);
        float s2 = MathUtils.Cross(rB, perp);

        Vector3 impulse = new Vector3();
        Vector2 C1 = new Vector2();
        C1.X = Vector2.Dot(perp, d);
        C1.Y = aB - aA - this.ReferenceAngle;

        float linearError = Math.Abs(C1.X);
        float angularError = Math.Abs(C1.Y);

        bool active = false;
        float C2 = 0.0f;
        if (this._enableLimit)
        {
            float translation = Vector2.Dot(axis, d);
            if (Math.Abs(this._upperTranslation - this._lowerTranslation) < 2.0f * Settings.LinearSlop)
            {
                C2 = translation;
                linearError = Math.Max(linearError, Math.Abs(translation));
                active = true;
            }
            else if (translation <= this._lowerTranslation)
            {
                C2 = Math.Min(translation - this._lowerTranslation, 0.0f);
                linearError = Math.Max(linearError, this._lowerTranslation - translation);
                active = true;
            }
            else if (translation >= this._upperTranslation)
            {
                C2 = Math.Max(translation - this._upperTranslation, 0.0f);
                linearError = Math.Max(linearError, translation - this._upperTranslation);
                active = true;
            }
        }

        if (active)
        {
            float k11 = mA + mB + iA * s1 * s1 + iB * s2 * s2;
            float k12 = iA * s1 + iB * s2;
            float k13 = iA * s1 * a1 + iB * s2 * a2;
            float k22 = iA + iB;
            if (k22.Equals(0.0f))
            {
                // For fixed rotation
                k22 = 1.0f;
            }

            float k23 = iA * a1 + iB * a2;
            float k33 = mA + mB + iA * a1 * a1 + iB * a2 * a2;

            Matrix3x3 K = new Matrix3x3();
            K.Ex.Set(k11, k12, k13);
            K.Ey.Set(k12, k22, k23);
            K.Ez.Set(k13, k23, k33);

            Vector3 C = new Vector3();
            C.X = C1.X;
            C.Y = C1.Y;
            C.Z = C2;

            impulse = K.Solve33(-C);
        }
        else
        {
            float k11 = mA + mB + iA * s1 * s1 + iB * s2 * s2;
            float k12 = iA * s1 + iB * s2;
            float k22 = iA + iB;
            if (k22.Equals(0.0f))
            {
                k22 = 1.0f;
            }

            Matrix2x2 K = new Matrix2x2();
            K.Ex.Set(k11, k12);
            K.Ey.Set(k12, k22);

            Vector2 impulse1 = K.Solve(-C1);
            impulse.X = impulse1.X;
            impulse.Y = impulse1.Y;
            impulse.Z = 0.0f;
        }

        Vector2 P = impulse.X * perp + impulse.Z * axis;
        float LA = impulse.X * s1 + impulse.Y + impulse.Z * a1;
        float LB = impulse.X * s2 + impulse.Y + impulse.Z * a2;

        cA -= mA * P;
        aA -= iA * LA;
        cB += mB * P;
        aB += iB * LB;

        data.Positions[this._indexA].Center = cA;
        data.Positions[this._indexA].Angle = aA;
        data.Positions[this._indexB].Center = cB;
        data.Positions[this._indexB].Angle = aB;

        return linearError <= Settings.LinearSlop && angularError <= Settings.AngularSlop;
    }

    /// <inheritdoc />
    public override void Draw(IDrawer drawer)
    {
        Transform xfA = this.BodyA.GetTransform();
        Transform xfB = this.BodyB.GetTransform();
        Vector2 pA = MathUtils.Mul(xfA, this.LocalAnchorA);
        Vector2 pB = MathUtils.Mul(xfB, this.LocalAnchorB);

        Vector2 axis = MathUtils.Mul(xfA.Rotation, this.LocalXAxisA);

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
            Vector2 perp = MathUtils.Mul(xfA.Rotation, this.LocalYAxisA);
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