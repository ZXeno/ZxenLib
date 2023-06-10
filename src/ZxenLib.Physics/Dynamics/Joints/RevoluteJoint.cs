namespace ZxenLib.Physics.Dynamics.Joints;

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Common;
using Color = Common.Color;

/// A revolute joint constrains two bodies to share a common point while they
/// are free to rotate about the point. The relative rotation about the shared
/// point is the joint angle. You can limit the relative rotation with
/// a joint limit that specifies a lower and upper angle. You can use a motor
/// to drive the relative rotation about the shared point. A maximum motor torque
/// is provided so that infinite forces are not generated.
public class RevoluteJoint : Joint
{
    internal readonly float ReferenceAngle;

    private bool _enableLimit;

    private bool _enableMotor;

    private Vector2 _impulse;

    // Solver temp
    private int _indexA;

    private int _indexB;

    private float _invIa;

    private float _invIb;

    private Matrix2x2 _K;

    private float _angle;

    private float _axialMass;

    private float _invMassA;

    private float _invMassB;

    private Vector2 _localCenterA;

    private Vector2 _localCenterB;

    private float _lowerAngle;

    private float _maxMotorTorque;

    private float _motorImpulse;

    private float _lowerImpulse;

    private float _upperImpulse;

    private float _motorSpeed;

    private Vector2 _rA;

    private Vector2 _rB;

    private float _upperAngle;

    // Solver shared
    internal Vector2 LocalAnchorA;

    internal Vector2 LocalAnchorB;

    internal RevoluteJoint(RevoluteJointDef def)
        : base(def)
    {
        this.LocalAnchorA = def.LocalAnchorA;
        this.LocalAnchorB = def.LocalAnchorB;
        this.ReferenceAngle = def.ReferenceAngle;

        this._impulse.SetZero();
        this._motorImpulse = 0.0f;
        this._axialMass = 0.0f;
        this._lowerImpulse = 0.0f;
        this._upperImpulse = 0.0f;

        this._lowerAngle = def.LowerAngle;
        this._upperAngle = def.UpperAngle;
        this._maxMotorTorque = def.MaxMotorTorque;
        this._motorSpeed = def.MotorSpeed;
        this._enableLimit = def.EnableLimit;
        this._enableMotor = def.EnableMotor;
        this._angle = 0.0f;
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

    /// Get the reference angle.
    public float GetReferenceAngle()
    {
        return this.ReferenceAngle;
    }

    /// Get the current joint angle in radians.
    public float GetJointAngle()
    {
        Body? bA = this.BodyA;
        Body? bB = this.BodyB;
        return bB.Sweep.A - bA.Sweep.A - this.ReferenceAngle;
    }

    /// Get the current joint angle speed in radians per second.
    public float GetJointSpeed()
    {
        Body? bA = this.BodyA;
        Body? bB = this.BodyB;
        return bB.AngularVelocity - bA.AngularVelocity;
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

    /// Get the lower joint limit in radians.
    public float GetLowerLimit()
    {
        return this._lowerAngle;
    }

    /// Get the upper joint limit in radians.
    public float GetUpperLimit()
    {
        return this._upperAngle;
    }

    /// Set the joint limits in radians.
    public void SetLimits(float lower, float upper)
    {
        Debug.Assert(lower <= upper);

        if (Math.Abs(lower - this._lowerAngle) > Settings.Epsilon || Math.Abs(upper - this._upperAngle) > Settings.Epsilon)
        {
            this.BodyA.IsAwake = true;
            this.BodyB.IsAwake = true;
            this._lowerImpulse = 0.0f;
            this._upperImpulse = 0.0f;
            this._lowerAngle = lower;
            this._upperAngle = upper;
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

    /// Set the motor speed in radians per second.
    public void SetMotorSpeed(float speed)
    {
        if (speed != this._motorSpeed)
        {
            this.BodyA.IsAwake = true;
            this.BodyB.IsAwake = true;
            this._motorSpeed = speed;
        }
    }

    /// Get the motor speed in radians per second.
    public float GetMotorSpeed()
    {
        return this._motorSpeed;
    }

    /// Set the maximum motor torque, usually in N-m.
    public void SetMaxMotorTorque(float torque)
    {
        if (torque != this._maxMotorTorque)
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

    /// Get the reaction force given the inverse time step.
    /// Unit is N.
    /// Get the current motor torque given the inverse time step.
    /// Unit is N*m.
    public float GetMotorTorque(float inv_dt)
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
        Vector2 P = new Vector2(this._impulse.X, this._impulse.Y);
        return inv_dt * P;
    }

    /// <inheritdoc />
    public override float GetReactionTorque(float inv_dt)
    {
        return inv_dt * (this._motorImpulse + this._lowerImpulse - this._upperImpulse);
    }

    /// Dump to Logger.Log.
    public override void Dump()
    {
        int indexA = this.BodyA.IslandIndex;
        int indexB = this.BodyB.IslandIndex;

        DumpLogger.Log("  b2RevoluteJointDef jd;");
        DumpLogger.Log($"  jd.bodyA = bodies[{indexA}];");
        DumpLogger.Log($"  jd.bodyB = bodies[{indexB}];");
        DumpLogger.Log($"  jd.collideConnected = bool({this.CollideConnected});");
        DumpLogger.Log($"  jd.localAnchorA.Set({this.LocalAnchorA.X}, {this.LocalAnchorA.Y});");
        DumpLogger.Log($"  jd.localAnchorB.Set({this.LocalAnchorB.X}, {this.LocalAnchorB.Y});");
        DumpLogger.Log($"  jd.referenceAngle = {this.ReferenceAngle};");
        DumpLogger.Log($"  jd.enableLimit = bool({this._enableLimit});");
        DumpLogger.Log($"  jd.lowerAngle = {this._lowerAngle};");
        DumpLogger.Log($"  jd.upperAngle = {this._upperAngle};");
        DumpLogger.Log($"  jd.enableMotor = bool({this._enableMotor});");
        DumpLogger.Log($"  jd.motorSpeed = {this._motorSpeed};");
        DumpLogger.Log($"  jd.maxMotorTorque = {this._maxMotorTorque};");
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

        float aA = data.Positions[this._indexA].Angle;
        Vector2 vA = data.Velocities[this._indexA].V;
        float wA = data.Velocities[this._indexA].W;

        float aB = data.Positions[this._indexB].Angle;
        Vector2 vB = data.Velocities[this._indexB].V;
        float wB = data.Velocities[this._indexB].W;

        Rotation qA = new Rotation(aA);
        Rotation qB = new Rotation(aB);

        this._rA = MathUtils.Mul(qA, this.LocalAnchorA - this._localCenterA);
        this._rB = MathUtils.Mul(qB, this.LocalAnchorB - this._localCenterB);

        // J = [-I -r1_skew I r2_skew]
        //     [ 0       -1 0       1]
        // r_skew = [-ry; rx]

        // Matlab
        // K = [ mA+r1y^2*iA+mB+r2y^2*iB,  -r1y*iA*r1x-r2y*iB*r2x]
        //     [  -r1y*iA*r1x-r2y*iB*r2x, mA+r1x^2*iA+mB+r2x^2*iB]

        float mA = this._invMassA, mB = this._invMassB;
        float iA = this._invIa, iB = this._invIb;

        this._K.Ex.X = mA + mB + this._rA.Y * this._rA.Y * iA + this._rB.Y * this._rB.Y * iB;
        this._K.Ey.X = -this._rA.Y * this._rA.X * iA - this._rB.Y * this._rB.X * iB;
        this._K.Ex.Y = this._K.Ey.X;
        this._K.Ey.Y = mA + mB + this._rA.X * this._rA.X * iA + this._rB.X * this._rB.X * iB;

        this._axialMass = iA + iB;
        bool fixedRotation;
        if (this._axialMass > 0.0f)
        {
            this._axialMass = 1.0f / this._axialMass;
            fixedRotation = false;
        }
        else
        {
            fixedRotation = true;
        }

        this._angle = aB - aA - this.ReferenceAngle;
        if (this._enableLimit == false || fixedRotation)
        {
            this._lowerImpulse = 0.0f;
            this._upperImpulse = 0.0f;
        }

        if (this._enableMotor == false || fixedRotation)
        {
            this._motorImpulse = 0.0f;
        }

        if (data.Step.WarmStarting)
        {
            // Scale impulses to support a variable time step.
            this._impulse *= data.Step.DtRatio;
            this._motorImpulse *= data.Step.DtRatio;
            this._lowerImpulse *= data.Step.DtRatio;
            this._upperImpulse *= data.Step.DtRatio;

            float axialImpulse = this._motorImpulse + this._lowerImpulse - this._upperImpulse;
            Vector2 P = new Vector2(this._impulse.X, this._impulse.Y);

            vA -= mA * P;
            wA -= iA * (MathUtils.Cross(this._rA, P) + axialImpulse);

            vB += mB * P;
            wB += iB * (MathUtils.Cross(this._rB, P) + axialImpulse);
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

    /// <inheritdoc />
    internal override void SolveVelocityConstraints(in SolverData data)
    {
        Vector2 vA = data.Velocities[this._indexA].V;
        float wA = data.Velocities[this._indexA].W;
        Vector2 vB = data.Velocities[this._indexB].V;
        float wB = data.Velocities[this._indexB].W;

        float mA = this._invMassA, mB = this._invMassB;
        float iA = this._invIa, iB = this._invIb;

        bool fixedRotation = (iA + iB).Equals(0.0f);

        // Solve motor constraint.
        if (this._enableMotor && fixedRotation == false)
        {
            float cdot = wB - wA - this._motorSpeed;
            float impulse = -this._axialMass * cdot;
            float oldImpulse = this._motorImpulse;
            float maxImpulse = data.Step.Dt * this._maxMotorTorque;
            this._motorImpulse = MathUtils.Clamp(this._motorImpulse + impulse, -maxImpulse, maxImpulse);
            impulse = this._motorImpulse - oldImpulse;

            wA -= iA * impulse;
            wB += iB * impulse;
        }

        if (this._enableLimit && fixedRotation == false)
        {
            // Lower limit
            {
                float C = this._angle - this._lowerAngle;
                float Cdot = wB - wA;
                float impulse = -this._axialMass * (Cdot + Math.Max(C, 0.0f) * data.Step.InvDt);
                float oldImpulse = this._lowerImpulse;
                this._lowerImpulse = Math.Max(this._lowerImpulse + impulse, 0.0f);
                impulse = this._lowerImpulse - oldImpulse;

                wA -= iA * impulse;
                wB += iB * impulse;
            }

            // Upper limit
            // Note: signs are flipped to keep C positive when the constraint is satisfied.
            // This also keeps the impulse positive when the limit is active.
            {
                float C = this._upperAngle - this._angle;
                float Cdot = wA - wB;
                float impulse = -this._axialMass * (Cdot + Math.Max(C, 0.0f) * data.Step.InvDt);
                float oldImpulse = this._upperImpulse;
                this._upperImpulse = Math.Max(this._upperImpulse + impulse, 0.0f);
                impulse = this._upperImpulse - oldImpulse;

                wA += iA * impulse;
                wB -= iB * impulse;
            }
        }

        // Solve point-to-point constraint
        {
            Vector2 Cdot = vB + MathUtils.Cross(wB, this._rB) - vA - MathUtils.Cross(wA, this._rA);
            Vector2 impulse = this._K.Solve(-Cdot);

            this._impulse.X += impulse.X;
            this._impulse.Y += impulse.Y;

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
        Vector2 cA = data.Positions[this._indexA].Center;
        float aA = data.Positions[this._indexA].Angle;
        Vector2 cB = data.Positions[this._indexB].Center;
        float aB = data.Positions[this._indexB].Angle;

        Rotation qA = new Rotation(aA);
        Rotation qB = new Rotation(aB);

        float angularError = 0.0f;
        float positionError = 0.0f;

        bool fixedRotation = (this._invIa + this._invIb).Equals(0.0f);

        // Solve angular limit constraint
        if (this._enableLimit && fixedRotation == false)
        {
            float angle = aB - aA - this.ReferenceAngle;
            float C = 0.0f;

            if (Math.Abs(this._upperAngle - this._lowerAngle) < 2.0f * Settings.AngularSlop)
            {
                // Prevent large angular corrections
                C = MathUtils.Clamp(angle - this._lowerAngle, -Settings.MaxAngularCorrection, Settings.MaxAngularCorrection);
            }
            else if (angle <= this._lowerAngle)
            {
                // Prevent large angular corrections and allow some slop.
                C = MathUtils.Clamp(angle - this._lowerAngle + Settings.AngularSlop, -Settings.MaxAngularCorrection, 0.0f);
            }
            else if (angle >= this._upperAngle)
            {
                // Prevent large angular corrections and allow some slop.
                C = MathUtils.Clamp(angle - this._upperAngle - Settings.AngularSlop, 0.0f, Settings.MaxAngularCorrection);
            }

            float limitImpulse = -this._axialMass * C;
            aA -= this._invIa * limitImpulse;
            aB += this._invIb * limitImpulse;
            angularError = Math.Abs(C);
        }

        // Solve point-to-point constraint.
        {
            qA.Set(aA);
            qB.Set(aB);
            Vector2 rA = MathUtils.Mul(qA, this.LocalAnchorA - this._localCenterA);
            Vector2 rB = MathUtils.Mul(qB, this.LocalAnchorB - this._localCenterB);

            Vector2 C = cB + rB - cA - rA;
            positionError = C.Length();

            float mA = this._invMassA, mB = this._invMassB;
            float iA = this._invIa, iB = this._invIb;

            Matrix2x2 K = new Matrix2x2();
            K.Ex.X = mA + mB + iA * rA.Y * rA.Y + iB * rB.Y * rB.Y;
            K.Ex.Y = -iA * rA.X * rA.Y - iB * rB.X * rB.Y;
            K.Ey.X = K.Ex.Y;
            K.Ey.Y = mA + mB + iA * rA.X * rA.X + iB * rB.X * rB.X;

            Vector2 impulse = -K.Solve(C);

            cA -= mA * impulse;
            aA -= iA * MathUtils.Cross(rA, impulse);

            cB += mB * impulse;
            aB += iB * MathUtils.Cross(rB, impulse);
        }

        data.Positions[this._indexA].Center = cA;
        data.Positions[this._indexA].Angle = aA;
        data.Positions[this._indexB].Center = cB;
        data.Positions[this._indexB].Angle = aB;

        return positionError <= Settings.LinearSlop && angularError <= Settings.AngularSlop;
    }

    /// <inheritdoc />
    public override void Draw(IDrawer drawer)
    {
        Transform xfA = this.BodyA.GetTransform();
        Transform xfB = this.BodyB.GetTransform();
        Vector2 pA = MathUtils.Mul(xfA, this.LocalAnchorA);
        Vector2 pB = MathUtils.Mul(xfB, this.LocalAnchorB);

        Color c1 = Color.FromArgb(0.7f, 0.7f, 0.7f);
        Color c2 = Color.FromArgb(0.3f, 0.9f, 0.3f);
        Color c3 = Color.FromArgb(0.9f, 0.3f, 0.3f);
        Color c4 = Color.FromArgb(0.3f, 0.3f, 0.9f);
        Color c5 = Color.FromArgb(0.4f, 0.4f, 0.4f);

        drawer.DrawPoint(pA, 5.0f, c4);
        drawer.DrawPoint(pB, 5.0f, c5);

        float aA = this.BodyA.GetAngle();
        float aB = this.BodyB.GetAngle();
        float angle = aB - aA - this.ReferenceAngle;

        float L = 0.5f;

        Vector2 r = L * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        drawer.DrawSegment(pB, pB + r, c1);
        drawer.DrawCircle(pB, L, c1);

        if (this._enableLimit)
        {
            Vector2 rlo = L * new Vector2((float)Math.Cos(this._lowerAngle), (float)Math.Cos(this._lowerAngle));
            Vector2 rhi = L * new Vector2((float)Math.Cos(this._upperAngle), (float)Math.Cos(this._upperAngle));

            drawer.DrawSegment(pB, pB + rlo, c2);
            drawer.DrawSegment(pB, pB + rhi, c3);
        }

        Color color = Color.FromArgb(0.5f, 0.8f, 0.8f);
        drawer.DrawSegment(xfA.Position, pA, color);
        drawer.DrawSegment(pA, pB, color);
        drawer.DrawSegment(xfB.Position, pB, color);
    }
}