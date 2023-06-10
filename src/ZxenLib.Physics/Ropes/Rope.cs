namespace ZxenLib.Physics.Ropes;

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Common;
using Color = Common.Color;

public class Rope
{
    private RopeBend[] _bendConstraints;

    private int _bendCount;

    private Vector2[] _bindPositions;

    private int _count;

    private Vector2 _gravity;

    private float[] _invMasses;

    private Vector2[] _p0s;

    private Vector2 _position;

    private Vector2[] _ps;

    private RopeStretch[] _stretchConstraints;

    private int _stretchCount;

    private RopeTuning _tuning;

    private Vector2[] _vs;

    public void Create(in RopeDef def)
    {
        Debug.Assert(def.Count >= 3);
        this._position = def.Position;
        this._count = def.Count;
        this._bindPositions = new Vector2[this._count];
        this._ps = new Vector2[this._count];
        this._p0s = new Vector2[this._count];
        this._vs = new Vector2[this._count];
        this._invMasses = new float[this._count];

        for (int i = 0; i < this._count; ++i)
        {
            this._bindPositions[i] = def.Vertices[i];
            this._ps[i] = def.Vertices[i] + this._position;
            this._p0s[i] = def.Vertices[i] + this._position;
            this._vs[i].SetZero();

            float m = def.Masses[i];
            if (m > 0.0f)
            {
                this._invMasses[i] = 1.0f / m;
            }
            else
            {
                this._invMasses[i] = 0.0f;
            }
        }

        this._stretchCount = this._count - 1;
        this._bendCount = this._count - 2;

        this._stretchConstraints = new RopeStretch[this._stretchCount];
        this._bendConstraints = new RopeBend[this._bendCount];

        for (int i = 0; i < this._stretchCount; ++i)
        {
            ref RopeStretch c = ref this._stretchConstraints[i];
            Vector2 p1 = this._ps[i];
            Vector2 p2 = this._ps[i + 1];

            c.I1 = i;
            c.I2 = i + 1;
            c.L = Vector2.Distance(p1, p2);
            c.InvMass1 = this._invMasses[i];
            c.InvMass2 = this._invMasses[i + 1];
            c.Lambda = 0.0f;
            c.Damper = 0.0f;
            c.Spring = 0.0f;
        }

        for (int i = 0; i < this._bendCount; ++i)
        {
            ref RopeBend c = ref this._bendConstraints[i];

            Vector2 p1 = this._ps[i];
            Vector2 p2 = this._ps[i + 1];
            Vector2 p3 = this._ps[i + 2];

            c.i1 = i;
            c.i2 = i + 1;
            c.i3 = i + 2;
            c.invMass1 = this._invMasses[i];
            c.invMass2 = this._invMasses[i + 1];
            c.invMass3 = this._invMasses[i + 2];
            c.invEffectiveMass = 0.0f;
            c.L1 = Vector2.Distance(p1, p2);
            c.L2 = Vector2.Distance(p2, p3);
            c.lambda = 0.0f;

            // Pre-compute effective mass (TODO use flattened config)
            Vector2 e1 = p2 - p1;
            Vector2 e2 = p3 - p2;
            float l1Sqr = e1.LengthSquared();
            float l2Sqr = e2.LengthSquared();

            if ((l1Sqr * l2Sqr).Equals(0))
            {
                continue;
            }

            Vector2 jd1 = -1.0f / l1Sqr * e1.Skew();
            Vector2 jd2 = 1.0f / l2Sqr * e2.Skew();

            Vector2 j1 = -jd1;
            Vector2 j2 = jd1 - jd2;
            Vector2 j3 = jd2;

            c.invEffectiveMass = c.invMass1 * Vector2.Dot(j1, j1) + c.invMass2 * Vector2.Dot(j2, j2) + c.invMass3 * Vector2.Dot(j3, j3);

            Vector2 r = p3 - p1;

            float rr = r.LengthSquared();
            if (rr.Equals(0))
            {
                continue;
            }

            // a1 = h2 / (h1 + h2)
            // a2 = h1 / (h1 + h2)
            c.alpha1 = Vector2.Dot(e2, r) / rr;
            c.alpha2 = Vector2.Dot(e1, r) / rr;
        }

        this._gravity = def.Gravity;

        this.SetTuning(def.Tuning);
    }

    public void SetTuning(RopeTuning tuning)
    {
        this._tuning = tuning;

        // Pre-compute spring and damper values based on tuning

        float bendOmega = 2.0f * Settings.Pi * this._tuning.BendHertz;

        for (int i = 0; i < this._bendCount; ++i)
        {
            ref RopeBend c = ref this._bendConstraints[i];

            float l1Sqr = c.L1 * c.L1;
            float l2Sqr = c.L2 * c.L2;

            if ((l1Sqr * l2Sqr).Equals(0))
            {
                c.spring = 0.0f;
                c.damper = 0.0f;
                continue;
            }

            // Flatten the triangle formed by the two edges
            float j2 = 1.0f / c.L1 + 1.0f / c.L2;
            float sum = c.invMass1 / l1Sqr + c.invMass2 * j2 * j2 + c.invMass3 / l2Sqr;
            if (sum.Equals(0))
            {
                c.spring = 0.0f;
                c.damper = 0.0f;
                continue;
            }

            float mass = 1.0f / sum;

            c.spring = mass * bendOmega * bendOmega;
            c.damper = 2.0f * mass * this._tuning.BendDamping * bendOmega;
        }

        float stretchOmega = 2.0f * Settings.Pi * this._tuning.StretchHertz;

        for (int i = 0; i < this._stretchCount; ++i)
        {
            ref RopeStretch c = ref this._stretchConstraints[i];

            float sum = c.InvMass1 + c.InvMass2;
            if (sum.Equals(0))
            {
                continue;
            }

            float mass = 1.0f / sum;

            c.Spring = mass * stretchOmega * stretchOmega;
            c.Damper = 2.0f * mass * this._tuning.StretchDamping * stretchOmega;
        }
    }

    public void Step(float dt, int iterations, Vector2 position)
    {
        if (dt.Equals(0))
        {
            return;
        }

        float invDt = 1.0f / dt;

        float d = (float)Math.Exp(-dt * this._tuning.Damping);

        // Apply gravity and damping
        for (int i = 0; i < this._count; ++i)
        {
            if (this._invMasses[i] > 0.0f)
            {
                this._vs[i] *= d;
                this._vs[i] += dt * this._gravity;
            }
            else
            {
                this._vs[i] = invDt * (this._bindPositions[i] + position - this._p0s[i]);
            }
        }

        // Apply bending spring
        if (this._tuning.BendingModel == BendingModel.SpringAngleBendingModel)
        {
            this.ApplyBendForces(dt);
        }

        for (int i = 0; i < this._bendCount; ++i)
        {
            this._bendConstraints[i].lambda = 0.0f;
        }

        for (int i = 0; i < this._stretchCount; ++i)
        {
            this._stretchConstraints[i].Lambda = 0.0f;
        }

        // Update position
        for (int i = 0; i < this._count; ++i)
        {
            this._ps[i] += dt * this._vs[i];
        }

        // Solve constraints
        for (int i = 0; i < iterations; ++i)
        {
            switch (this._tuning.BendingModel)
            {
                case BendingModel.PbdAngleBendingModel:
                    this.SolveBend_PBD_Angle();
                    break;
                case BendingModel.XpdAngleBendingModel:
                    this.SolveBend_XPBD_Angle(dt);
                    break;
                case BendingModel.PbdDistanceBendingModel:
                    this.SolveBend_PBD_Distance();
                    break;
                case BendingModel.PbdHeightBendingModel:
                    this.SolveBend_PBD_Height();
                    break;
                case BendingModel.PbdTriangleBendingModel:
                    this.SolveBend_PBD_Triangle();
                    break;
            }

            switch (this._tuning.StretchingModel)
            {
                case StretchingModel.PbdStretchingModel:
                    this.SolveStretch_PBD();
                    break;
                case StretchingModel.XpbdStretchingModel:
                    this.SolveStretch_XPBD(dt);
                    break;
            }
        }

        // Constrain velocity
        for (int i = 0; i < this._count; ++i)
        {
            this._vs[i] = invDt * (this._ps[i] - this._p0s[i]);
            this._p0s[i] = this._ps[i];
        }
    }

    public void Reset(Vector2 position)
    {
        this._position = position;

        for (int i = 0; i < this._count; ++i)
        {
            this._ps[i] = this._bindPositions[i] + this._position;
            this._p0s[i] = this._bindPositions[i] + this._position;
            this._vs[i].SetZero();
        }

        for (int i = 0; i < this._bendCount; ++i)
        {
            this._bendConstraints[i].lambda = 0.0f;
        }

        for (int i = 0; i < this._stretchCount; ++i)
        {
            this._stretchConstraints[i].Lambda = 0.0f;
        }
    }

    private void SolveStretch_PBD()
    {
        float stiffness = this._tuning.StretchStiffness;
        for (int i = 0; i < this._stretchCount; ++i)
        {
            ref RopeStretch c = ref this._stretchConstraints[i];

            Vector2 p1 = this._ps[c.I1];
            Vector2 p2 = this._ps[c.I2];

            Vector2 d = p2 - p1;
            float l = MathExtensions.Normalize(ref d);

            float sum = c.InvMass1 + c.InvMass2;
            if (sum.Equals(0))
            {
                continue;
            }

            float s1 = c.InvMass1 / sum;
            float s2 = c.InvMass2 / sum;

            p1 -= stiffness * s1 * (c.L - l) * d;
            p2 += stiffness * s2 * (c.L - l) * d;

            this._ps[c.I1] = p1;
            this._ps[c.I2] = p2;
        }
    }

    private void SolveStretch_XPBD(float dt)
    {
        Debug.Assert(dt > 0.0f);
        for (int i = 0; i < this._stretchCount; ++i)
        {
            ref RopeStretch ropeStretch = ref this._stretchConstraints[i];

            Vector2 p1 = this._ps[ropeStretch.I1];
            Vector2 p2 = this._ps[ropeStretch.I2];

            Vector2 dp1 = p1 - this._p0s[ropeStretch.I1];
            Vector2 dp2 = p2 - this._p0s[ropeStretch.I2];

            Vector2 u = p2 - p1;
            float l = MathExtensions.Normalize(ref u);

            Vector2 j1 = -u;
            Vector2 j2 = u;

            float sum = ropeStretch.InvMass1 + ropeStretch.InvMass2;
            if (sum.Equals(0))
            {
                continue;
            }

            float alpha = 1.0f / (ropeStretch.Spring * dt * dt); // 1 / kg
            float beta = dt * dt * ropeStretch.Damper;           // kg * s
            float sigma = alpha * beta / dt;                     // non-dimensional
            float stretchL = l - ropeStretch.L;

            // This is using the initial velocities
            float cDot = Vector2.Dot(j1, dp1) + Vector2.Dot(j2, dp2);

            float b = stretchL + alpha * ropeStretch.Lambda + sigma * cDot;
            float sum2 = (1.0f + sigma) * sum + alpha;

            float impulse = -b / sum2;

            p1 += ropeStretch.InvMass1 * impulse * j1;
            p2 += ropeStretch.InvMass2 * impulse * j2;

            this._ps[ropeStretch.I1] = p1;
            this._ps[ropeStretch.I2] = p2;
            ropeStretch.Lambda += impulse;
        }
    }

    private void SolveBend_PBD_Angle()
    {
        float stiffness = this._tuning.BendStiffness;
        for (int i = 0; i < this._bendCount; ++i)
        {
            ref RopeBend c = ref this._bendConstraints[i];

            Vector2 p1 = this._ps[c.i1];
            Vector2 p2 = this._ps[c.i2];
            Vector2 p3 = this._ps[c.i3];

            Vector2 d1 = p2 - p1;
            Vector2 d2 = p3 - p2;
            float a = MathUtils.Cross(d1, d2);
            float b = Vector2.Dot(d1, d2);

            float angle = (float)Math.Atan2(a, b);

            float L1sqr, L2sqr;

            if (this._tuning.Isometric)
            {
                L1sqr = c.L1 * c.L1;
                L2sqr = c.L2 * c.L2;
            }
            else
            {
                L1sqr = d1.LengthSquared();
                L2sqr = d2.LengthSquared();
            }

            if ((L1sqr * L2sqr).Equals(0))
            {
                continue;
            }

            Vector2 Jd1 = -1.0f / L1sqr * d1.Skew();
            Vector2 Jd2 = 1.0f / L2sqr * d2.Skew();

            Vector2 J1 = -Jd1;
            Vector2 J2 = Jd1 - Jd2;
            Vector2 J3 = Jd2;

            float sum;
            if (this._tuning.FixedEffectiveMass)
            {
                sum = c.invEffectiveMass;
            }
            else
            {
                sum = c.invMass1 * Vector2.Dot(J1, J1) + c.invMass2 * Vector2.Dot(J2, J2) + c.invMass3 * Vector2.Dot(J3, J3);
            }

            if (sum.Equals(0))
            {
                sum = c.invEffectiveMass;
            }

            float impulse = -stiffness * angle / sum;

            p1 += c.invMass1 * impulse * J1;
            p2 += c.invMass2 * impulse * J2;
            p3 += c.invMass3 * impulse * J3;

            this._ps[c.i1] = p1;
            this._ps[c.i2] = p2;
            this._ps[c.i3] = p3;
        }
    }

    private void SolveBend_XPBD_Angle(float dt)
    {
        Debug.Assert(dt > 0.0f);
        for (int i = 0; i < this._bendCount; ++i)
        {
            ref RopeBend c = ref this._bendConstraints[i];

            Vector2 p1 = this._ps[c.i1];
            Vector2 p2 = this._ps[c.i2];
            Vector2 p3 = this._ps[c.i3];

            Vector2 dp1 = p1 - this._p0s[c.i1];
            Vector2 dp2 = p2 - this._p0s[c.i2];
            Vector2 dp3 = p3 - this._p0s[c.i3];

            Vector2 d1 = p2 - p1;
            Vector2 d2 = p3 - p2;

            float L1sqr, L2sqr;

            if (this._tuning.Isometric)
            {
                L1sqr = c.L1 * c.L1;
                L2sqr = c.L2 * c.L2;
            }
            else
            {
                L1sqr = d1.LengthSquared();
                L2sqr = d2.LengthSquared();
            }

            if ((L1sqr * L2sqr).Equals(0))
            {
                continue;
            }

            float a = MathUtils.Cross(d1, d2);
            float b = Vector2.Dot(d1, d2);

            float angle = (float)Math.Atan2(a, b);

            Vector2 Jd1 = -1.0f / L1sqr * d1.Skew();
            Vector2 Jd2 = 1.0f / L2sqr * d2.Skew();

            Vector2 J1 = -Jd1;
            Vector2 J2 = Jd1 - Jd2;
            Vector2 J3 = Jd2;

            float sum;
            if (this._tuning.FixedEffectiveMass)
            {
                sum = c.invEffectiveMass;
            }
            else
            {
                sum = c.invMass1 * Vector2.Dot(J1, J1) + c.invMass2 * Vector2.Dot(J2, J2) + c.invMass3 * Vector2.Dot(J3, J3);
            }

            if (sum.Equals(0))
            {
                continue;
            }

            float alpha = 1.0f / (c.spring * dt * dt);
            float beta = dt * dt * c.damper;
            float sigma = alpha * beta / dt;
            float C = angle;

            // This is using the initial velocities
            float Cdot = Vector2.Dot(J1, dp1) + Vector2.Dot(J2, dp2) + Vector2.Dot(J3, dp3);

            float B = C + alpha * c.lambda + sigma * Cdot;
            float sum2 = (1.0f + sigma) * sum + alpha;

            float impulse = -B / sum2;

            p1 += c.invMass1 * impulse * J1;
            p2 += c.invMass2 * impulse * J2;
            p3 += c.invMass3 * impulse * J3;

            this._ps[c.i1] = p1;
            this._ps[c.i2] = p2;
            this._ps[c.i3] = p3;
            c.lambda += impulse;
        }
    }

    private void ApplyBendForces(float dt)
    {
        // omega = 2 * pi * hz
        float omega = 2.0f * Settings.Pi * this._tuning.BendHertz;
        for (int i = 0; i < this._bendCount; ++i)
        {
            ref RopeBend c = ref this._bendConstraints[i];

            Vector2 p1 = this._ps[c.i1];
            Vector2 p2 = this._ps[c.i2];
            Vector2 p3 = this._ps[c.i3];

            Vector2 v1 = this._vs[c.i1];
            Vector2 v2 = this._vs[c.i2];
            Vector2 v3 = this._vs[c.i3];

            Vector2 d1 = p2 - p1;
            Vector2 d2 = p3 - p2;

            float L1sqr, L2sqr;

            if (this._tuning.Isometric)
            {
                L1sqr = c.L1 * c.L1;
                L2sqr = c.L2 * c.L2;
            }
            else
            {
                L1sqr = d1.LengthSquared();
                L2sqr = d2.LengthSquared();
            }

            if ((L1sqr * L2sqr).Equals(0))
            {
                continue;
            }

            float a = MathUtils.Cross(d1, d2);
            float b = Vector2.Dot(d1, d2);

            float angle = (float)Math.Atan2(a, b);

            Vector2 Jd1 = -1.0f / L1sqr * d1.Skew();
            Vector2 Jd2 = 1.0f / L2sqr * d2.Skew();

            Vector2 J1 = -Jd1;
            Vector2 J2 = Jd1 - Jd2;
            Vector2 J3 = Jd2;

            float sum;
            if (this._tuning.FixedEffectiveMass)
            {
                sum = c.invEffectiveMass;
            }
            else
            {
                sum = c.invMass1 * Vector2.Dot(J1, J1) + c.invMass2 * Vector2.Dot(J2, J2) + c.invMass3 * Vector2.Dot(J3, J3);
            }

            if (sum.Equals(0))
            {
                continue;
            }

            float mass = 1.0f / sum;

            float spring = mass * omega * omega;
            float damper = 2.0f * mass * this._tuning.BendDamping * omega;

            float C = angle;
            float Cdot = Vector2.Dot(J1, v1) + Vector2.Dot(J2, v2) + Vector2.Dot(J3, v3);

            float impulse = -dt * (spring * C + damper * Cdot);

            this._vs[c.i1] += c.invMass1 * impulse * J1;
            this._vs[c.i2] += c.invMass2 * impulse * J2;
            this._vs[c.i3] += c.invMass3 * impulse * J3;
        }
    }

    private void SolveBend_PBD_Distance()
    {
        float stiffness = this._tuning.BendStiffness;
        for (int i = 0; i < this._bendCount; ++i)
        {
            ref RopeBend c = ref this._bendConstraints[i];

            int i1 = c.i1;
            int i2 = c.i3;

            Vector2 p1 = this._ps[i1];
            Vector2 p2 = this._ps[i2];

            Vector2 d = p2 - p1;
            float L = MathExtensions.Normalize(ref d);

            float sum = c.invMass1 + c.invMass3;
            if (sum.Equals(0))
            {
                continue;
            }

            float s1 = c.invMass1 / sum;
            float s2 = c.invMass3 / sum;

            p1 -= stiffness * s1 * (c.L1 + c.L2 - L) * d;
            p2 += stiffness * s2 * (c.L1 + c.L2 - L) * d;

            this._ps[i1] = p1;
            this._ps[i2] = p2;
        }
    }

    // Constraint based implementation of:
    // P. Volino: Simple Linear Bending Stiffness in Particle Systems
    private void SolveBend_PBD_Height()
    {
        float stiffness = this._tuning.BendStiffness;
        for (int i = 0; i < this._bendCount; ++i)
        {
            ref RopeBend c = ref this._bendConstraints[i];

            Vector2 p1 = this._ps[c.i1];
            Vector2 p2 = this._ps[c.i2];
            Vector2 p3 = this._ps[c.i3];

            // Barycentric coordinates are held constant
            Vector2 d = c.alpha1 * p1 + c.alpha2 * p3 - p2;
            float dLen = d.Length();

            if (dLen.Equals(0))
            {
                continue;
            }

            Vector2 dHat = 1.0f / dLen * d;

            Vector2 J1 = c.alpha1 * dHat;
            Vector2 J2 = -dHat;
            Vector2 J3 = c.alpha2 * dHat;

            float sum = c.invMass1 * c.alpha1 * c.alpha1 + c.invMass2 + c.invMass3 * c.alpha2 * c.alpha2;

            if (sum.Equals(0))
            {
                continue;
            }

            float C = dLen;
            float mass = 1.0f / sum;
            float impulse = -stiffness * mass * C;

            p1 += c.invMass1 * impulse * J1;
            p2 += c.invMass2 * impulse * J2;
            p3 += c.invMass3 * impulse * J3;

            this._ps[c.i1] = p1;
            this._ps[c.i2] = p2;
            this._ps[c.i3] = p3;
        }
    }

    // M. Kelager: A Triangle Bending Constraint Model for PBD
    private void SolveBend_PBD_Triangle()
    {
        float stiffness = this._tuning.BendStiffness;

        for (int i = 0; i < this._bendCount; ++i)
        {
            RopeBend c = this._bendConstraints[i];

            Vector2 b0 = this._ps[c.i1];
            Vector2 v = this._ps[c.i2];
            Vector2 b1 = this._ps[c.i3];

            float wb0 = c.invMass1;
            float wv = c.invMass2;
            float wb1 = c.invMass3;

            float W = wb0 + wb1 + 2.0f * wv;
            float invW = stiffness / W;

            Vector2 d = v - (1.0f / 3.0f) * (b0 + v + b1);

            Vector2 db0 = 2.0f * wb0 * invW * d;
            Vector2 dv = -4.0f * wv * invW * d;
            Vector2 db1 = 2.0f * wb1 * invW * d;

            b0 += db0;
            v += dv;
            b1 += db1;

            this._ps[c.i1] = b0;
            this._ps[c.i2] = v;
            this._ps[c.i3] = b1;
        }
    }

    public void Draw(IDrawer draw)
    {
        Color c = Color.FromArgb(0.4f, 0.5f, 0.7f);

        Color pg = Color.FromArgb(0.1f, 0.8f, 0.1f);

        Color pd = Color.FromArgb(0.7f, 0.2f, 0.4f);
        for (int i = 0; i < this._count - 1; ++i)
        {
            draw.DrawSegment(this._ps[i], this._ps[i + 1], c);

            Color pc = this._invMasses[i] > 0.0f ? pd : pg;
            draw.DrawPoint(this._ps[i], 5.0f, pc);
        }

        {
            Color pc = this._invMasses[this._count - 1] > 0.0f ? pd : pg;
            draw.DrawPoint(this._ps[this._count - 1], 5.0f, pc);
        }
    }
}