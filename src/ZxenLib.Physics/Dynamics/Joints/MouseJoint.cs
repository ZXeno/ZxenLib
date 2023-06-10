namespace ZxenLib.Physics.Dynamics.Joints;

using Microsoft.Xna.Framework;
using Common;

/// A mouse joint is used to make a point on a body track a
/// specified world point. This a soft constraint with a maximum
/// force. This allows the constraint to stretch and without
/// applying huge forces.
/// NOTE: this joint is not documented in the manual because it was
/// developed to be used in the testbed. If you want to learn how to
/// use the mouse joint, look at the testbed.
public class MouseJoint : Joint
{
    private readonly Vector2 _localAnchorB;

    private float _beta;

    private Vector2 _C;

    public float Damping;

    public float Stiffness;

    private float _gamma;

    // Solver shared
    private Vector2 _impulse;

    // Solver temp
    private int _indexB;

    private float _invIb;

    private float _invMassB;

    private Vector2 _localCenterB;

    private Matrix2x2 _mass;

    public float MaxForce;

    private Vector2 _rB;

    public Vector2 Target;

    internal MouseJoint(MouseJointDef def)
        : base(def)
    {
        this.Target = def.Target;
        this._localAnchorB = MathUtils.MulT(this.BodyB.GetTransform(), this.Target);

        this.MaxForce = def.MaxForce;
        this.Stiffness = def.Stiffness;
        this.Damping = def.Damping;

        this._impulse.SetZero();
        this._beta = 0.0f;
        this._gamma = 0.0f;
    }

    /// Implements b2Joint.
    /// Use this to update the target point.
    public void SetTarget(in Vector2 target)
    {
        if (target != this.Target)
        {
            this.BodyB.IsAwake = true;
            this.Target = target;
        }
    }

    /// <inheritdoc />
    public override void ShiftOrigin(in Vector2 newOrigin)
    {
        this.Target -= newOrigin;
    }

    /// <inheritdoc />
    public override Vector2 GetAnchorA()
    {
        return this.Target;
    }

    /// <inheritdoc />
    public override Vector2 GetAnchorB()
    {
        return this.BodyB.GetWorldPoint(this._localAnchorB);
    }

    /// <inheritdoc />
    public override Vector2 GetReactionForce(float inv_dt)
    {
        return inv_dt * this._impulse;
    }

    /// <inheritdoc />
    public override float GetReactionTorque(float inv_dt)
    {
        return inv_dt * 0.0f;
    }

    /// The mouse joint does not support dumping.
    public override void Dump()
    {
        DumpLogger.Log("Mouse joint dumping is not supported.");
    }

    /// <inheritdoc />
    internal override void InitVelocityConstraints(in SolverData data)
    {
        this._indexB = this.BodyB.IslandIndex;
        this._localCenterB = this.BodyB.Sweep.LocalCenter;
        this._invMassB = this.BodyB.InvMass;
        this._invIb = this.BodyB.InverseInertia;

        Vector2 cB = data.Positions[this._indexB].Center;
        float aB = data.Positions[this._indexB].Angle;
        Vector2 vB = data.Velocities[this._indexB].V;
        float wB = data.Velocities[this._indexB].W;

        Rotation qB = new Rotation(aB);

        float d = this.Damping;
        float k = this.Stiffness;

        // magic formulas
        // gamma has units of inverse mass.
        // beta has units of inverse time.
        float h = data.Step.Dt;

        this._gamma = h * (d + h * k);
        if (!this._gamma.Equals(0.0f))
        {
            this._gamma = 1.0f / this._gamma;
        }

        this._beta = h * k * this._gamma;

        // Compute the effective mass matrix.
        this._rB = MathUtils.Mul(qB, this._localAnchorB - this._localCenterB);

        // K    = [(1/m1 + 1/m2) * eye(2) - skew(r1) * invI1 * skew(r1) - skew(r2) * invI2 * skew(r2)]
        //      = [1/m1+1/m2     0    ] + invI1 * [r1.Y*r1.Y -r1.X*r1.Y] + invI2 * [r1.Y*r1.Y -r1.X*r1.Y]
        //        [    0     1/m1+1/m2]           [-r1.X*r1.Y r1.X*r1.X]           [-r1.X*r1.Y r1.X*r1.X]
        Matrix2x2 K = new Matrix2x2();
        K.Ex.X = this._invMassB + this._invIb * this._rB.Y * this._rB.Y + this._gamma;
        K.Ex.Y = -this._invIb * this._rB.X * this._rB.Y;
        K.Ey.X = K.Ex.Y;
        K.Ey.Y = this._invMassB + this._invIb * this._rB.X * this._rB.X + this._gamma;

        this._mass = K.GetInverse();

        this._C = cB + this._rB - this.Target;
        this._C *= this._beta;

        // Cheat with some damping
        wB *= 0.98f;

        if (data.Step.WarmStarting)
        {
            this._impulse *= data.Step.DtRatio;
            vB += this._invMassB * this._impulse;
            wB += this._invIb * MathUtils.Cross(this._rB, this._impulse);
        }
        else
        {
            this._impulse.SetZero();
        }

        data.Velocities[this._indexB].V = vB;
        data.Velocities[this._indexB].W = wB;
    }

    /// <inheritdoc />
    internal override void SolveVelocityConstraints(in SolverData data)
    {
        Vector2 vB = data.Velocities[this._indexB].V;
        float wB = data.Velocities[this._indexB].W;

        // Cdot = v + cross(w, r)
        Vector2 cdot = vB + MathUtils.Cross(wB, this._rB);
        Vector2 impulse = MathUtils.Mul(this._mass, -(cdot + this._C + this._gamma * this._impulse));

        Vector2 oldImpulse = this._impulse;
        this._impulse += impulse;
        float maxImpulse = data.Step.Dt * this.MaxForce;
        if (this._impulse.LengthSquared() > maxImpulse * maxImpulse)
        {
            this._impulse *= maxImpulse / this._impulse.Length();
        }

        impulse = this._impulse - oldImpulse;

        vB += this._invMassB * impulse;
        wB += this._invIb * MathUtils.Cross(this._rB, impulse);

        data.Velocities[this._indexB].V = vB;
        data.Velocities[this._indexB].W = wB;
    }

    /// <inheritdoc />
    internal override bool SolvePositionConstraints(in SolverData data)
    {
        return true;
    }
}