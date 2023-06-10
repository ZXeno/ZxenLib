namespace ZxenLib.Physics.Dynamics.Joints;

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Common;

/// A gear joint is used to connect two joints together. Either joint
/// can be a revolute or prismatic joint. You specify a gear ratio
/// to bind the motions together:
/// coordinate1 + ratio * coordinate2 = constant
/// The ratio can be negative or positive. If one joint is a revolute joint
/// and the other joint is a prismatic joint, then the ratio will have units
/// of length or units of 1/length.
/// @warning You have to manually destroy the gear joint if joint1 or joint2
/// is destroyed.
public class GearJoint : Joint
{
    // Body A is connected to body C
    // Body B is connected to body D
    private readonly Body _bodyC;

    private readonly Body _bodyD;

    private readonly float _constant;

    private readonly Joint _joint1;

    private readonly Joint _joint2;

    // Solver shared
    private readonly Vector2 _localAnchorA;

    private readonly Vector2 _localAnchorB;

    private readonly Vector2 _localAnchorC;

    private readonly Vector2 _localAnchorD;

    private readonly Vector2 _localAxisC;

    private readonly Vector2 _localAxisD;

    private readonly float _referenceAngleA;

    private readonly float _referenceAngleB;

    private readonly JointType _typeA;

    private readonly JointType _typeB;

    private float _iA, _iB, _iC, _iD;

    private float _impulse;

    // Solver temp
    private int _indexA, _indexB, _indexC, _indexD;

    private Vector2 _jvAc, _jvBd;

    private float _jwA, _jwB, _jwC, _jwD;

    private Vector2 _lcA, _lcB, _lcC, _lcD;

    private float _mA, _mB, _mC, _mD;

    private float _mass;

    private float _ratio;

    private float _tolerance;

    public GearJoint(GearJointDef def)
        : base(def)
    {
        this._joint1 = def.Joint1;
        this._joint2 = def.Joint2;

        this._typeA = this._joint1.JointType;
        this._typeB = this._joint2.JointType;

        Debug.Assert(this._typeA == JointType.RevoluteJoint || this._typeA == JointType.PrismaticJoint);
        Debug.Assert(this._typeB == JointType.RevoluteJoint || this._typeB == JointType.PrismaticJoint);

        float coordinateA, coordinateB;

        // TODO_ERIN there might be some problem with the joint edges in b2Joint.

        this._bodyC = this._joint1.BodyA;
        this.BodyA = this._joint1.BodyB;

        // Body B on joint1 must be dynamic
        Debug.Assert(this.BodyA.BodyType == BodyType.DynamicBody);

        // Get geometry of joint1
        Transform xfA = this.BodyA.Transform;
        float aA = this.BodyA.Sweep.A;
        Transform xfC = this._bodyC.Transform;
        float aC = this._bodyC.Sweep.A;

        if (this._typeA == JointType.RevoluteJoint)
        {
            RevoluteJoint? revolute = (RevoluteJoint)def.Joint1;
            this._localAnchorC = revolute.LocalAnchorA;
            this._localAnchorA = revolute.LocalAnchorB;
            this._referenceAngleA = revolute.ReferenceAngle;
            this._localAxisC.SetZero();

            coordinateA = aA - aC - this._referenceAngleA;

            // position error is measured in radians
            this._tolerance = Settings.AngularSlop;
        }
        else
        {
            PrismaticJoint? prismatic = (PrismaticJoint)def.Joint1;
            this._localAnchorC = prismatic.LocalAnchorA;
            this._localAnchorA = prismatic.LocalAnchorB;
            this._referenceAngleA = prismatic.ReferenceAngle;
            this._localAxisC = prismatic.LocalXAxisA;

            Vector2 pC = this._localAnchorC;
            Vector2 pA = MathUtils.MulT(
                xfC.Rotation,
                MathUtils.Mul(xfA.Rotation, this._localAnchorA) + (xfA.Position - xfC.Position));
            coordinateA = Vector2.Dot(pA - pC, this._localAxisC);

            // position error is measured in meters
            this._tolerance = Settings.LinearSlop;
        }

        this._bodyD = this._joint2.BodyA;
        this.BodyB = this._joint2.BodyB;

        // Body B on joint2 must be dynamic
        Debug.Assert(this.BodyB.BodyType == BodyType.DynamicBody);

        // Get geometry of joint2
        Transform xfB = this.BodyB.Transform;
        float aB = this.BodyB.Sweep.A;
        Transform xfD = this._bodyD.Transform;
        float aD = this._bodyD.Sweep.A;

        if (this._typeB == JointType.RevoluteJoint)
        {
            RevoluteJoint? revolute = (RevoluteJoint)def.Joint2;
            this._localAnchorD = revolute.LocalAnchorA;
            this._localAnchorB = revolute.LocalAnchorB;
            this._referenceAngleB = revolute.ReferenceAngle;
            this._localAxisD.SetZero();

            coordinateB = aB - aD - this._referenceAngleB;
        }
        else
        {
            PrismaticJoint? prismatic = (PrismaticJoint)def.Joint2;
            this._localAnchorD = prismatic.LocalAnchorA;
            this._localAnchorB = prismatic.LocalAnchorB;
            this._referenceAngleB = prismatic.ReferenceAngle;
            this._localAxisD = prismatic.LocalXAxisA;

            Vector2 pD = this._localAnchorD;
            Vector2 pB = MathUtils.MulT(
                xfD.Rotation,
                MathUtils.Mul(xfB.Rotation, this._localAnchorB) + (xfB.Position - xfD.Position));
            coordinateB = Vector2.Dot(pB - pD, this._localAxisD);
        }

        this._ratio = def.Ratio;

        this._constant = coordinateA + this._ratio * coordinateB;

        this._impulse = 0.0f;
    }

    /// Get the first joint.
    public Joint GetJoint1()
    {
        return this._joint1;
    }

    /// Get the second joint.
    public Joint GetJoint2()
    {
        return this._joint2;
    }

    /// Set/Get the gear ratio.
    public void SetRatio(float ratio)
    {
        Debug.Assert(ratio.IsValid());
        this._ratio = ratio;
    }

    public float GetRatio()
    {
        return this._ratio;
    }

    /// <inheritdoc />
    public override void Dump()
    {
        int indexA = this.BodyA.IslandIndex;
        int indexB = this.BodyB.IslandIndex;

        int index1 = this._joint1.Index;
        int index2 = this._joint2.Index;

        DumpLogger.Log("  b2GearJointDef jd;");
        DumpLogger.Log($"  jd.bodyA = bodies[{indexA}];");
        DumpLogger.Log($"  jd.bodyB = bodies[{indexB}];");
        DumpLogger.Log($"  jd.collideConnected = bool({this.CollideConnected});");
        DumpLogger.Log("  jd.joint1 = joints[index1];");
        DumpLogger.Log($"  jd.joint2 = joints[{index2}];");
        DumpLogger.Log($"  jd.ratio = {this._ratio};");
        DumpLogger.Log($"  joints[{this.Index}] = m_world.CreateJoint(&jd);");
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
        Vector2 P = this._impulse * this._jvAc;
        return inv_dt * P;
    }

    /// <inheritdoc />
    public override float GetReactionTorque(float inv_dt)
    {
        float L = this._impulse * this._jwA;
        return inv_dt * L;
    }

    /// <inheritdoc />
    internal override void InitVelocityConstraints(in SolverData data)
    {
        this._indexA = this.BodyA.IslandIndex;
        this._indexB = this.BodyB.IslandIndex;
        this._indexC = this._bodyC.IslandIndex;
        this._indexD = this._bodyD.IslandIndex;
        this._lcA = this.BodyA.Sweep.LocalCenter;
        this._lcB = this.BodyB.Sweep.LocalCenter;
        this._lcC = this._bodyC.Sweep.LocalCenter;
        this._lcD = this._bodyD.Sweep.LocalCenter;
        this._mA = this.BodyA.InvMass;
        this._mB = this.BodyB.InvMass;
        this._mC = this._bodyC.InvMass;
        this._mD = this._bodyD.InvMass;
        this._iA = this.BodyA.InverseInertia;
        this._iB = this.BodyB.InverseInertia;
        this._iC = this._bodyC.InverseInertia;
        this._iD = this._bodyD.InverseInertia;

        float aA = data.Positions[this._indexA].Angle;
        Vector2 vA = data.Velocities[this._indexA].V;
        float wA = data.Velocities[this._indexA].W;

        float aB = data.Positions[this._indexB].Angle;
        Vector2 vB = data.Velocities[this._indexB].V;
        float wB = data.Velocities[this._indexB].W;

        float aC = data.Positions[this._indexC].Angle;
        Vector2 vC = data.Velocities[this._indexC].V;
        float wC = data.Velocities[this._indexC].W;

        float aD = data.Positions[this._indexD].Angle;
        Vector2 vD = data.Velocities[this._indexD].V;
        float wD = data.Velocities[this._indexD].W;

        Rotation qA = new Rotation(aA), qB = new Rotation(aB), qC = new Rotation(aC), qD = new Rotation(aD);

        this._mass = 0.0f;

        if (this._typeA == JointType.RevoluteJoint)
        {
            this._jvAc.SetZero();
            this._jwA = 1.0f;
            this._jwC = 1.0f;
            this._mass += this._iA + this._iC;
        }
        else
        {
            Vector2 u = MathUtils.Mul(qC, this._localAxisC);
            Vector2 rC = MathUtils.Mul(qC, this._localAnchorC - this._lcC);
            Vector2 rA = MathUtils.Mul(qA, this._localAnchorA - this._lcA);
            this._jvAc = u;
            this._jwC = MathUtils.Cross(rC, u);
            this._jwA = MathUtils.Cross(rA, u);
            this._mass += this._mC + this._mA + this._iC * this._jwC * this._jwC + this._iA * this._jwA * this._jwA;
        }

        if (this._typeB == JointType.RevoluteJoint)
        {
            this._jvBd.SetZero();
            this._jwB = this._ratio;
            this._jwD = this._ratio;
            this._mass += this._ratio * this._ratio * (this._iB + this._iD);
        }
        else
        {
            Vector2 u = MathUtils.Mul(qD, this._localAxisD);
            Vector2 rD = MathUtils.Mul(qD, this._localAnchorD - this._lcD);
            Vector2 rB = MathUtils.Mul(qB, this._localAnchorB - this._lcB);
            this._jvBd = this._ratio * u;
            this._jwD = this._ratio * MathUtils.Cross(rD, u);
            this._jwB = this._ratio * MathUtils.Cross(rB, u);
            this._mass += this._ratio * this._ratio * (this._mD + this._mB) + this._iD * this._jwD * this._jwD + this._iB * this._jwB * this._jwB;
        }

        // Compute effective mass.
        this._mass = this._mass > 0.0f ? 1.0f / this._mass : 0.0f;

        if (data.Step.WarmStarting)
        {
            vA += this._mA * this._impulse * this._jvAc;
            wA += this._iA * this._impulse * this._jwA;
            vB += this._mB * this._impulse * this._jvBd;
            wB += this._iB * this._impulse * this._jwB;
            vC -= this._mC * this._impulse * this._jvAc;
            wC -= this._iC * this._impulse * this._jwC;
            vD -= this._mD * this._impulse * this._jvBd;
            wD -= this._iD * this._impulse * this._jwD;
        }
        else
        {
            this._impulse = 0.0f;
        }

        data.Velocities[this._indexA].V = vA;
        data.Velocities[this._indexA].W = wA;
        data.Velocities[this._indexB].V = vB;
        data.Velocities[this._indexB].W = wB;
        data.Velocities[this._indexC].V = vC;
        data.Velocities[this._indexC].W = wC;
        data.Velocities[this._indexD].V = vD;
        data.Velocities[this._indexD].W = wD;
    }

    /// <inheritdoc />
    internal override void SolveVelocityConstraints(in SolverData data)
    {
        Vector2 vA = data.Velocities[this._indexA].V;
        float wA = data.Velocities[this._indexA].W;
        Vector2 vB = data.Velocities[this._indexB].V;
        float wB = data.Velocities[this._indexB].W;
        Vector2 vC = data.Velocities[this._indexC].V;
        float wC = data.Velocities[this._indexC].W;
        Vector2 vD = data.Velocities[this._indexD].V;
        float wD = data.Velocities[this._indexD].W;

        float Cdot = Vector2.Dot(this._jvAc, vA - vC) + Vector2.Dot(this._jvBd, vB - vD);
        Cdot += this._jwA * wA - this._jwC * wC + (this._jwB * wB - this._jwD * wD);

        float impulse = -this._mass * Cdot;
        this._impulse += impulse;

        vA += this._mA * impulse * this._jvAc;
        wA += this._iA * impulse * this._jwA;
        vB += this._mB * impulse * this._jvBd;
        wB += this._iB * impulse * this._jwB;
        vC -= this._mC * impulse * this._jvAc;
        wC -= this._iC * impulse * this._jwC;
        vD -= this._mD * impulse * this._jvBd;
        wD -= this._iD * impulse * this._jwD;

        data.Velocities[this._indexA].V = vA;
        data.Velocities[this._indexA].W = wA;
        data.Velocities[this._indexB].V = vB;
        data.Velocities[this._indexB].W = wB;
        data.Velocities[this._indexC].V = vC;
        data.Velocities[this._indexC].W = wC;
        data.Velocities[this._indexD].V = vD;
        data.Velocities[this._indexD].W = wD;
    }

    /// <inheritdoc />
    internal override bool SolvePositionConstraints(in SolverData data)
    {
        Vector2 cA = data.Positions[this._indexA].Center;
        float aA = data.Positions[this._indexA].Angle;
        Vector2 cB = data.Positions[this._indexB].Center;
        float aB = data.Positions[this._indexB].Angle;
        Vector2 cC = data.Positions[this._indexC].Center;
        float aC = data.Positions[this._indexC].Angle;
        Vector2 cD = data.Positions[this._indexD].Center;
        float aD = data.Positions[this._indexD].Angle;

        Rotation qA = new Rotation(aA);
        Rotation qB = new Rotation(aB);
        Rotation qC = new Rotation(aC);
        Rotation qD = new Rotation(aD);


        float coordinateA, coordinateB;

        Vector2 JvAC = new Vector2();
        Vector2 JvBD = new Vector2();
        float JwA, JwB, JwC, JwD;
        float mass = 0.0f;

        if (this._typeA == JointType.RevoluteJoint)
        {
            JvAC.SetZero();
            JwA = 1.0f;
            JwC = 1.0f;
            mass += this._iA + this._iC;

            coordinateA = aA - aC - this._referenceAngleA;
        }
        else
        {
            Vector2 u = MathUtils.Mul(qC, this._localAxisC);
            Vector2 rC = MathUtils.Mul(qC, this._localAnchorC - this._lcC);
            Vector2 rA = MathUtils.Mul(qA, this._localAnchorA - this._lcA);
            JvAC = u;
            JwC = MathUtils.Cross(rC, u);
            JwA = MathUtils.Cross(rA, u);
            mass += this._mC + this._mA + this._iC * JwC * JwC + this._iA * JwA * JwA;

            Vector2 pC = this._localAnchorC - this._lcC;
            Vector2 pA = MathUtils.MulT(qC, rA + (cA - cC));
            coordinateA = Vector2.Dot(pA - pC, this._localAxisC);
        }

        if (this._typeB == JointType.RevoluteJoint)
        {
            JvBD.SetZero();
            JwB = this._ratio;
            JwD = this._ratio;
            mass += this._ratio * this._ratio * (this._iB + this._iD);

            coordinateB = aB - aD - this._referenceAngleB;
        }
        else
        {
            Vector2 u = MathUtils.Mul(qD, this._localAxisD);
            Vector2 rD = MathUtils.Mul(qD, this._localAnchorD - this._lcD);
            Vector2 rB = MathUtils.Mul(qB, this._localAnchorB - this._lcB);
            JvBD = this._ratio * u;
            JwD = this._ratio * MathUtils.Cross(rD, u);
            JwB = this._ratio * MathUtils.Cross(rB, u);
            mass += this._ratio * this._ratio * (this._mD + this._mB) + this._iD * JwD * JwD + this._iB * JwB * JwB;

            Vector2 pD = this._localAnchorD - this._lcD;
            Vector2 pB = MathUtils.MulT(qD, rB + (cB - cD));
            coordinateB = Vector2.Dot(pB - pD, this._localAxisD);
        }

        float C = coordinateA + this._ratio * coordinateB - this._constant;

        float impulse = 0.0f;
        if (mass > 0.0f)
        {
            impulse = -C / mass;
        }

        cA += this._mA * impulse * JvAC;
        aA += this._iA * impulse * JwA;
        cB += this._mB * impulse * JvBD;
        aB += this._iB * impulse * JwB;
        cC -= this._mC * impulse * JvAC;
        aC -= this._iC * impulse * JwC;
        cD -= this._mD * impulse * JvBD;
        aD -= this._iD * impulse * JwD;

        data.Positions[this._indexA].Center = cA;
        data.Positions[this._indexA].Angle = aA;
        data.Positions[this._indexB].Center = cB;
        data.Positions[this._indexB].Angle = aB;
        data.Positions[this._indexC].Center = cC;
        data.Positions[this._indexC].Angle = aC;
        data.Positions[this._indexD].Center = cD;
        data.Positions[this._indexD].Angle = aD;

        return Math.Abs(C) < this._tolerance;
    }
}