namespace ZxenLib.Physics.Dynamics.Contacts;

using System;
using System.Buffers;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Collision.Collider;
using Collision.Shapes;
using Common;

public class ContactSolver
{
    internal ContactPositionConstraint[] PositionConstraints;

    internal ContactVelocityConstraint[] VelocityConstraints;

    private int _contactCount;

    private Contact[] _contacts;

    private Position[] _positions;

    private Velocity[] _velocities;

    public void Setup(in ContactSolverDef def)
    {
        TimeStep step = def.Step;
        this._contactCount = def.ContactCount;
        this.PositionConstraints = ArrayPool<ContactPositionConstraint>.Shared.Rent(this._contactCount);
        this.VelocityConstraints = ArrayPool<ContactVelocityConstraint>.Shared.Rent(this._contactCount);

        this._positions = def.Positions;
        this._velocities = def.Velocities;
        this._contacts = def.Contacts;
        Span<Contact> contacts = this._contacts;
        Span<ContactVelocityConstraint> velocityConstraints = this.VelocityConstraints;
        Span<ContactPositionConstraint> positionConstraints = this.PositionConstraints;

        // Initialize position independent portions of the constraints.
        for (int i = 0; i < this._contactCount; ++i)
        {
            Contact? contact = contacts[i];

            Fixture? fixtureA = contact.FixtureA;
            Fixture? fixtureB = contact.FixtureB;
            Shape? shapeA = fixtureA.Shape;
            Shape? shapeB = fixtureB.Shape;
            float radiusA = shapeA.Radius;
            float radiusB = shapeB.Radius;
            Body? bodyA = fixtureA.Body;
            Body? bodyB = fixtureB.Body;
            ref Manifold manifold = ref contact.Manifold;

            int pointCount = manifold.PointCount;
            Debug.Assert(pointCount > 0);

            ref ContactVelocityConstraint vc = ref velocityConstraints[i];
            vc.Friction = contact.Friction;
            vc.Restitution = contact.Restitution;
            vc.Threshold = contact.RestitutionThreshold;
            vc.TangentSpeed = contact.TangentSpeed;
            vc.IndexA = bodyA.IslandIndex;
            vc.IndexB = bodyB.IslandIndex;
            vc.InvMassA = bodyA.InvMass;
            vc.InvMassB = bodyB.InvMass;
            vc.InvIa = bodyA.InverseInertia;
            vc.InvIb = bodyB.InverseInertia;
            vc.ContactIndex = i;
            vc.PointCount = pointCount;
            vc.K.SetZero();
            vc.NormalMass.SetZero();

            ref ContactPositionConstraint pc = ref positionConstraints[i];
            pc.IndexA = bodyA.IslandIndex;
            pc.IndexB = bodyB.IslandIndex;
            pc.InvMassA = bodyA.InvMass;
            pc.InvMassB = bodyB.InvMass;
            pc.LocalCenterA = bodyA.Sweep.LocalCenter;
            pc.LocalCenterB = bodyB.Sweep.LocalCenter;
            pc.InvIa = bodyA.InverseInertia;
            pc.InvIb = bodyB.InverseInertia;
            pc.LocalNormal = manifold.LocalNormal;
            pc.LocalPoint = manifold.LocalPoint;
            pc.PointCount = pointCount;
            pc.RadiusA = radiusA;
            pc.RadiusB = radiusB;
            pc.Type = manifold.Type;

            for (int j = 0; j < pointCount; ++j)
            {
                ref readonly ManifoldPoint cp = ref j == 0 ? ref manifold.Points.Value0 : ref manifold.Points.Value1;
                ref VelocityConstraintPoint vcp = ref j == 0 ? ref vc.Points.Value0 : ref vc.Points.Value1;

                if (step.WarmStarting)
                {
                    vcp.NormalImpulse = step.DtRatio * cp.NormalImpulse;
                    vcp.TangentImpulse = step.DtRatio * cp.TangentImpulse;
                }
                else
                {
                    vcp.NormalImpulse = 0.0f;
                    vcp.TangentImpulse = 0.0f;
                }

                vcp.Ra = default;
                vcp.Rb = default;
                vcp.NormalMass = 0.0f;
                vcp.TangentMass = 0.0f;
                vcp.VelocityBias = 0.0f;

                pc.LocalPoints[j] = cp.LocalPoint;
            }
        }
    }

    public void Reset()
    {
        Array.Clear(this.PositionConstraints, 0, this._contactCount);
        ArrayPool<ContactPositionConstraint>.Shared.Return(this.PositionConstraints);
        this.PositionConstraints = null;
        Array.Clear(this.VelocityConstraints, 0, this._contactCount);
        ArrayPool<ContactVelocityConstraint>.Shared.Return(this.VelocityConstraints);
        this.VelocityConstraints = null;
        this._positions = null;
        this._contacts = null;
        this._velocities = null;
        this._contactCount = 0;
    }

    ~ContactSolver()
    {
        if (this.PositionConstraints != null)
        {
            ArrayPool<ContactPositionConstraint>.Shared.Return(this.PositionConstraints, true);
        }

        if (this.VelocityConstraints != null)
        {
            ArrayPool<ContactVelocityConstraint>.Shared.Return(this.VelocityConstraints, true);
        }
    }

    public void InitializeVelocityConstraints()
    {
        Span<Position> ps = this._positions;
        Span<Velocity> vs = this._velocities;
        for (int i = 0; i < this._contactCount; ++i)
        {
            ref ContactVelocityConstraint vc = ref this.VelocityConstraints[i];
            ref ContactPositionConstraint pc = ref this.PositionConstraints[i];

            float radiusA = pc.RadiusA;
            float radiusB = pc.RadiusB;
            ref readonly Manifold manifold = ref this._contacts[vc.ContactIndex].Manifold;

            int indexA = vc.IndexA;
            int indexB = vc.IndexB;

            float mA = vc.InvMassA;
            float mB = vc.InvMassB;
            float iA = vc.InvIa;
            float iB = vc.InvIb;
            Vector2 localCenterA = pc.LocalCenterA;
            Vector2 localCenterB = pc.LocalCenterB;

            Vector2 cA = ps[indexA].Center;
            float aA = ps[indexA].Angle;
            Vector2 vA = vs[indexA].V;
            float wA = vs[indexA].W;

            Vector2 cB = ps[indexB].Center;
            float aB = ps[indexB].Angle;
            Vector2 vB = vs[indexB].V;
            float wB = vs[indexB].W;

            Debug.Assert(manifold.PointCount > 0);

            Transform xfA = new Transform();
            Transform xfB = new Transform();
            xfA.Rotation.Set(aA);
            xfB.Rotation.Set(aB);
            xfA.Position = cA - MathUtils.Mul(xfA.Rotation, localCenterA);
            xfB.Position = cB - MathUtils.Mul(xfB.Rotation, localCenterB);

            WorldManifold worldManifold = new WorldManifold();
            worldManifold.Initialize(
                manifold,
                xfA,
                radiusA,
                xfB,
                radiusB);

            vc.Normal = worldManifold.Normal;

            for (int j = 0; j < vc.PointCount; ++j)
            {
                ref VelocityConstraintPoint vcp = ref j == 0 ? ref vc.Points.Value0 : ref vc.Points.Value1;
                ref readonly Vector2 worldPoint = ref j == 0 ? ref worldManifold.Points.Value0 : ref worldManifold.Points.Value1;
                vcp.Ra = worldPoint - cA;
                vcp.Rb = worldPoint - cB;

                float rnA = MathUtils.Cross(vcp.Ra, vc.Normal);
                float rnB = MathUtils.Cross(vcp.Rb, vc.Normal);

                float kNormal = mA + mB + iA * rnA * rnA + iB * rnB * rnB;

                vcp.NormalMass = kNormal > 0.0f ? 1.0f / kNormal : 0.0f;

                Vector2 tangent = MathUtils.Cross(vc.Normal, 1.0f);

                float rtA = MathUtils.Cross(vcp.Ra, tangent);
                float rtB = MathUtils.Cross(vcp.Rb, tangent);

                float kTangent = mA + mB + iA * rtA * rtA + iB * rtB * rtB;

                vcp.TangentMass = kTangent > 0.0f ? 1.0f / kTangent : 0.0f;

                // Setup a velocity bias for restitution.
                vcp.VelocityBias = 0.0f;

                // var vRel = Vector2.Dot(
                //     vc.Normal,
                //     vB + MathUtils.Cross(wB, vcp.Rb) - vA - MathUtils.Cross(wA, vcp.Ra)); // inline
                float vRel = Vector2.Dot(
                    vc.Normal,
                    new Vector2(vB.X - wB * vcp.Rb.Y - vA.X + wA * vcp.Ra.Y, vB.Y + wB * vcp.Rb.X - vA.Y - wA * vcp.Ra.X));
                if (vRel < -vc.Threshold)
                {
                    vcp.VelocityBias = -vc.Restitution * vRel;
                }
            }

            // If we have two points, then prepare the block solver.
            if (vc.PointCount == 2)
            {
                ref readonly VelocityConstraintPoint vcp1 = ref vc.Points.Value0;
                ref readonly VelocityConstraintPoint vcp2 = ref vc.Points.Value1;

                float rn1A = vcp1.Ra.X * vc.Normal.Y - vcp1.Ra.Y * vc.Normal.X; // MathUtils.Cross(vcp1.Ra, vc.Normal);
                float rn1B = vcp1.Rb.X * vc.Normal.Y - vcp1.Rb.Y * vc.Normal.X; // MathUtils.Cross(vcp1.Rb, vc.Normal);
                float rn2A = vcp2.Ra.X * vc.Normal.Y - vcp2.Ra.Y * vc.Normal.X; // MathUtils.Cross(vcp2.Ra, vc.Normal);
                float rn2B = vcp2.Rb.X * vc.Normal.Y - vcp2.Rb.Y * vc.Normal.X; // MathUtils.Cross(vcp2.Rb, vc.Normal);

                float k11 = mA + mB + iA * rn1A * rn1A + iB * rn1B * rn1B;
                float k22 = mA + mB + iA * rn2A * rn2A + iB * rn2B * rn2B;
                float k12 = mA + mB + iA * rn1A * rn2A + iB * rn1B * rn2B;

                // Ensure a reasonable condition number.
                const float maxConditionNumber = 1000.0f;
                if (k11 * k11 < maxConditionNumber * (k11 * k22 - k12 * k12))
                {
                    // K is safe to invert.
                    vc.K.Ex.Set(k11, k12);
                    vc.K.Ey.Set(k12, k22);
                    vc.NormalMass = vc.K.GetInverse();
                }
                else
                {
                    // The constraints are redundant, just use one.
                    // TODO_ERIN use deepest?
                    vc.PointCount = 1;
                }
            }
        }
    }

    public void WarmStart()
    {
        // Warm start.
        Span<ContactVelocityConstraint> velocityConstraints = this.VelocityConstraints;
        Span<Velocity> velocities = this._velocities;
        for (int i = 0; i < this._contactCount; ++i)
        {
            ref ContactVelocityConstraint vc = ref velocityConstraints[i];

            int indexA = vc.IndexA;
            int indexB = vc.IndexB;
            float mA = vc.InvMassA;
            float iA = vc.InvIa;
            float mB = vc.InvMassB;
            float iB = vc.InvIb;
            int pointCount = vc.PointCount;

            Vector2 vA = velocities[indexA].V;
            float wA = velocities[indexA].W;
            Vector2 vB = velocities[indexB].V;
            float wB = velocities[indexB].W;

            Vector2 normal = vc.Normal;
            Vector2 tangent = MathUtils.Cross(normal, 1.0f);

            for (int j = 0; j < pointCount; ++j)
            {
                ref readonly VelocityConstraintPoint vcp = ref j == 0 ? ref vc.Points.Value0 : ref vc.Points.Value1;
                Vector2 P = vcp.NormalImpulse * normal + vcp.TangentImpulse * tangent;
                wA -= iA * MathUtils.Cross(vcp.Ra, P);
                vA -= mA * P;
                wB += iB * MathUtils.Cross(vcp.Rb, P);
                vB += mB * P;
            }

            velocities[indexA].V = vA;
            velocities[indexA].W = wA;
            velocities[indexB].V = vB;
            velocities[indexB].W = wB;
        }
    }

    public void SolveVelocityConstraints()
    {
        Span<ContactVelocityConstraint> velocityConstraints = this.VelocityConstraints;
        Span<Velocity> velocities = this._velocities;
        float tangentX, tangentY;
        for (int i = 0; i < this._contactCount; ++i)
        {
            ref ContactVelocityConstraint vc = ref velocityConstraints[i];

            int indexA = vc.IndexA;
            int indexB = vc.IndexB;
            float mA = vc.InvMassA;
            float iA = vc.InvIa;
            float mB = vc.InvMassB;
            float iB = vc.InvIb;
            int pointCount = vc.PointCount;
            ref readonly Velocity vsA = ref velocities[indexA];
            ref readonly Velocity vsB = ref velocities[indexB];
            float vAX = vsA.V.X;
            float vAY = vsA.V.Y;
            float wA = vsA.W;
            float vBX = vsB.V.X;
            float vBY = vsB.V.Y;
            float wB = vsB.W;

            float normalX = vc.Normal.X;
            float normalY = vc.Normal.Y;

            // var tangent = MathUtils.Cross(normal, 1.0f); // inline
            tangentX = normalY;
            tangentY = -normalX;

            float friction = vc.Friction;

            Debug.Assert(pointCount == 1 || pointCount == 2);

            // Solve tangent constraints first because non-penetration is more important
            // than friction.
            float dvX, dvY;
            float pX, pY;

            for (int j = 0; j < pointCount; ++j)
            {
                ref VelocityConstraintPoint vcp = ref j == 0 ? ref vc.Points.Value0 : ref vc.Points.Value1;

                // Relative velocity at contact
                //var dv = vB + MathUtils.Cross(wB, vcp.Rb) - vA - MathUtils.Cross(wA, vcp.Ra); // inline
                dvX = vBX - wB * vcp.Rb.Y - vAX + wA * vcp.Ra.Y;
                dvY = vBY + wB * vcp.Rb.X - vAY - wA * vcp.Ra.X;

                // Compute tangent force
                float vt = dvX * tangentX + dvY * tangentY - vc.TangentSpeed;
                float lambda = vcp.TangentMass * -vt;

                // MathUtils.b2Clamp the accumulated force
                float maxFriction = friction * vcp.NormalImpulse;

                //var newImpulse = MathUtils.Clamp(vcp.TangentImpulse + lambda, -maxFriction, maxFriction);
                float newImpulse = vcp.TangentImpulse + lambda;
                newImpulse = newImpulse < -maxFriction ? -maxFriction : newImpulse > maxFriction ? maxFriction : newImpulse;
                lambda = newImpulse - vcp.TangentImpulse;

                vcp.TangentImpulse = newImpulse;

                // Apply contact impulse
                pX = lambda * tangentX;
                pY = lambda * tangentY;

                vAX -= mA * pX;
                vAY -= mA * pY;

                // wA -= iA * MathUtils.Cross(vcp.Ra, P); // inline
                wA -= iA * (vcp.Ra.X * pY - vcp.Ra.Y * pX);

                vBX += mB * pX;
                vBY += mB * pY;

                // wB += iB * MathUtils.Cross(vcp.Rb, P); // inline
                wB += iB * (vcp.Rb.X * pY - vcp.Rb.Y * pX);
            }

            float P1X, P1Y, P2X, P2Y;

            // Solve normal constraints
            if (pointCount == 1)
            {
                ref VelocityConstraintPoint vcp = ref vc.Points.Value0;

                // Relative velocity at contact
                //var dv = vB + MathUtils.Cross(wB, vcp.Rb) - vA - MathUtils.Cross(wA, vcp.Ra); // inline
                dvX = vBX - wB * vcp.Rb.Y - vAX + wA * vcp.Ra.Y;
                dvY = vBY + wB * vcp.Rb.X - vAY - wA * vcp.Ra.X;

                // Compute normal impulse
                float vn = dvX * normalX + dvY * normalY;
                float lambda = -vcp.NormalMass * (vn - vcp.VelocityBias);

                // MathUtils.b2Clamp the accumulated impulse
                float newImpulse = Math.Max(vcp.NormalImpulse + lambda, 0.0f);
                lambda = newImpulse - vcp.NormalImpulse;
                vcp.NormalImpulse = newImpulse;

                // Apply contact impulse
                pX = lambda * normalX;
                pY = lambda * normalY;
                vAX -= mA * pX;
                vAY -= mA * pY;

                // wA -= iA * MathUtils.Cross(vcp.Ra, P); // inline
                wA -= iA * (vcp.Ra.X * pY - vcp.Ra.Y * pX);
                vBX += mB * pX;
                vBY += mB * pY;

                // wB += iB * MathUtils.Cross(vcp.Rb, P); // inline
                wB += iB * (vcp.Rb.X * pY - vcp.Rb.Y * pX);
            }
            else
            {
                // Block solver developed in collaboration with Dirk Gregorius (back in 01/07 on Box2D_Lite).
                // Build the mini LCP for this contact patch
                //
                // vn = A * x + b, vn >= 0, x >= 0 and vn_i * x_i = 0 with i = 1..2
                //
                // A = J * W * JT and J = ( -n, -r1 x n, n, r2 x n )
                // b = vn0 - velocityBias
                //
                // The system is solved using the "Total enumeration method" (s. Murty). The complementary constraint vn_i * x_i
                // implies that we must have in any solution either vn_i = 0 or x_i = 0. So for the 2D contact problem the cases
                // vn1 = 0 and vn2 = 0, x1 = 0 and x2 = 0, x1 = 0 and vn2 = 0, x2 = 0 and vn1 = 0 need to be tested. The first valid
                // solution that satisfies the problem is chosen.
                //
                // In order to account of the accumulated impulse 'a' (because of the iterative nature of the solver which only requires
                // that the accumulated impulse is clamped and not the incremental impulse) we change the impulse variable (x_i).
                //
                // Substitute:
                //
                // x = a + d
                //
                // a := old total impulse
                // x := new total impulse
                // d := incremental impulse
                //
                // For the current iteration we extend the formula for the incremental impulse
                // to compute the new total impulse:
                //
                // vn = A * d + b
                //    = A * (x - a) + b
                //    = A * x + b - A * a
                //    = A * x + b'
                // b' = b - A * a;

                //ref var cp1 = ref vc.Points.Value0;
                //ref var cp2 = ref vc.Points.Value1;
                float cp1VelocityBias = vc.Points.Value0.VelocityBias;
                float cp2VelocityBias = vc.Points.Value1.VelocityBias;
                float cp1NormalMass = vc.Points.Value0.NormalMass;
                float cp2NormalMass = vc.Points.Value1.NormalMass;
                float cp1RaX = vc.Points.Value0.Ra.X, cp1RaY = vc.Points.Value0.Ra.Y, cp1RbX = vc.Points.Value0.Rb.X, cp1RbY = vc.Points.Value0.Rb.Y;
                float cp2RaX = vc.Points.Value1.Ra.X, cp2RaY = vc.Points.Value1.Ra.Y, cp2RbX = vc.Points.Value1.Rb.X, cp2RbY = vc.Points.Value1.Rb.Y;
                ref float cp1NormalImpulse = ref vc.Points.Value0.NormalImpulse;
                ref float cp2NormalImpulse = ref vc.Points.Value1.NormalImpulse;
                Vector2 a = new Vector2(cp1NormalImpulse, cp2NormalImpulse);
                Debug.Assert(a.X >= 0.0f && a.Y >= 0.0f);

                // Relative velocity at contact
                float dv1X = vBX - wB * cp1RbY - vAX + wA * cp1RaY;
                float dv1Y = vBY + wB * cp1RbX - vAY - wA * cp1RaX;

                float dv2X = vBX - wB * cp2RbY - vAX + wA * cp2RaY;
                float dv2Y = vBY + wB * cp2RbX - vAY - wA * cp2RaX;

                // Compute normal velocity
                float vn1 = dv1X * normalX + dv1Y * normalY; // Vector2.Dot(dv1, normal);
                float vn2 = dv2X * normalX + dv2Y * normalY; //Vector2.Dot(dv2, normal);

                //var b = new Vector2(vn1 - cp1.VelocityBias, vn2 - cp2.VelocityBias); // inline
                Vector2 b = new Vector2(vn1 - cp1VelocityBias - (vc.K.Ex.X * a.X + vc.K.Ey.X * a.Y), vn2 - cp2VelocityBias - (vc.K.Ex.Y * a.X + vc.K.Ey.Y * a.Y));

                // Compute b'
                // b -= MathUtils.Mul(vc.K, a); // inline

                for (;;)
                {
                    //
                    // Case 1: vn = 0
                    //
                    // 0 = A * x + b'
                    //
                    // Solve for x:
                    //
                    // x = - inv(A) * b'
                    //
                    // var x = -MathUtils.Mul(vc.NormalMass, b);
                    Vector2 x = new Vector2(-(vc.NormalMass.Ex.X * b.X + vc.NormalMass.Ey.X * b.Y), -(vc.NormalMass.Ex.Y * b.X + vc.NormalMass.Ey.Y * b.Y));
                    if (x.X >= 0.0f && x.Y >= 0.0f)
                    {
                        // Get the incremental impulse
                        float dX = x.X - a.X;
                        float dY = x.Y - a.Y;

                        // Apply incremental impulse
                        P1X = dX * normalX;
                        P1Y = dX * normalY;
                        P2X = dY * normalX;
                        P2Y = dY * normalY;
                        vAX -= mA * (P1X + P2X);
                        vAY -= mA * (P1Y + P2Y);

                        //wA -= iA * (MathUtils.Cross(cp1.Ra, P1) + MathUtils.Cross(cp2.Ra, P2)); // inline
                        wA -= iA * (cp1RaX * P1Y - cp1RaY * P1X + (cp2RaX * P2Y - cp2RaY * P2X));

                        vBX += mB * (P1X + P2X);
                        vBY += mB * (P1Y + P2Y);

                        // wB += iB * (MathUtils.Cross(cp1.Rb, P1) + MathUtils.Cross(cp2.Rb, P2)); // inline
                        wB += iB * (cp1RbX * P1Y - cp1RbY * P1X + (cp2RbX * P2Y - cp2RbY * P2X));

                        // Accumulate
                        cp1NormalImpulse = x.X;
                        cp2NormalImpulse = x.Y;

                        #if B2_DEBUG_SOLVER
// Postconditions
                            const float k_errorTol = 1e-3f;
                            dv1 = vB + MathUtils.Cross(wB, cp1.rB) - vA - MathUtils.Cross(wA, cp1.rA);
                            dv2 = vB + MathUtils.Cross(wB, cp2.rB) - vA - MathUtils.Cross(wA, cp2.rA);

                            // Compute normal velocity
                            vn1 = Vector2.Dot(dv1, normal);
                            vn2 = Vector2.Dot(dv2, normal);

                            Debug.Assert(Math.Abs(vn1 - cp1.velocityBias) < k_errorTol);
                            Debug.Assert(Math.Abs(vn2 - cp2.velocityBias) < k_errorTol);
                        #endif
                        break;
                    }

                    //
                    // Case 2: vn1 = 0 and x2 = 0
                    //
                    //   0 = a11 * x1 + a12 * 0 + b1'
                    // vn2 = a21 * x1 + a22 * 0 + b2'
                    //
                    x.X = -cp1NormalMass * b.X;
                    x.Y = 0.0f;
                    vn1 = 0.0f;
                    vn2 = vc.K.Ex.Y * x.X + b.Y;
                    if (x.X >= 0.0f && vn2 >= 0.0f)
                    {
                        // Get the incremental impulse
                        float dX = x.X - a.X;
                        float dY = x.Y - a.Y;

                        // Apply incremental impulse
                        P1X = dX * normalX;
                        P1Y = dX * normalY;
                        P2X = dY * normalX;
                        P2Y = dY * normalY;
                        vAX -= mA * (P1X + P2X);
                        vAY -= mA * (P1Y + P2Y);

                        // wA -= iA * (MathUtils.Cross(cp1.Ra, P1) + MathUtils.Cross(cp2.Ra, P2)); //inline
                        wA -= iA * (cp1RaX * P1Y - cp1RaY * P1X + (cp2RaX * P2Y - cp2RaY * P2X));

                        vBX += mB * (P1X + P2X);
                        vBY += mB * (P1Y + P2Y);

                        // wB += iB * (MathUtils.Cross(cp1.Rb, P1) + MathUtils.Cross(cp2.Rb, P2)); // inline
                        wB += iB * (cp1RbX * P1Y - cp1RbY * P1X + (cp2RbX * P2Y - cp2RbY * P2X));

                        // Accumulate
                        cp1NormalImpulse = x.X;
                        cp2NormalImpulse = x.Y;

                        #if B2_DEBUG_SOLVER
// Postconditions
                            dv1 = vB + MathUtils.Cross(wB, cp1.rB) - vA - MathUtils.Cross(wA, cp1.rA);

                            // Compute normal velocity
                            vn1 = Vector2.Dot(dv1, normal);

                            Debug.Assert(Math.Abs(vn1 - cp1.velocityBias) < k_errorTol);
                        #endif
                        break;
                    }

                    //
                    // Case 3: vn2 = 0 and x1 = 0
                    //
                    // vn1 = a11 * 0 + a12 * x2 + b1'
                    //   0 = a21 * 0 + a22 * x2 + b2'
                    //
                    x.X = 0.0f;
                    x.Y = -cp2NormalMass * b.Y;
                    vn1 = vc.K.Ey.X * x.Y + b.X;
                    vn2 = 0.0f;

                    if (x.Y >= 0.0f && vn1 >= 0.0f)
                    {
                        // Resubstitute for the incremental impulse
                        float dX = x.X - a.X;
                        float dY = x.Y - a.Y;

                        // Apply incremental impulse
                        P1X = dX * normalX;
                        P1Y = dX * normalY;
                        P2X = dY * normalX;
                        P2Y = dY * normalY;
                        vAX -= mA * (P1X + P2X);
                        vAY -= mA * (P1Y + P2Y);

                        // wA -= iA * (MathUtils.Cross(cp1.Ra, P1) + MathUtils.Cross(cp2.Ra, P2)); // inline
                        wA -= iA * (cp1RaX * P1Y - cp1RaY * P1X + (cp2RaX * P2Y - cp2RaY * P2X));

                        vBX += mB * (P1X + P2X);
                        vBY += mB * (P1Y + P2Y);

                        // wB += iB * (MathUtils.Cross(cp1.Rb, P1) + MathUtils.Cross(cp2.Rb, P2)); // inline
                        wB += iB * (cp1RbX * P1Y - cp1RbY * P1X + (cp2RbX * P2Y - cp2RbY * P2X));

                        // Accumulate
                        cp1NormalImpulse = x.X;
                        cp2NormalImpulse = x.Y;

                        #if B2_DEBUG_SOLVER
// Postconditions
                            dv2 = vB + MathUtils.Cross(wB, cp2.rB) - vA - MathUtils.Cross(wA, cp2.rA);

                            // Compute normal velocity
                            vn2 = Vector2.Dot(dv2, normal);

                            Debug.Assert(Math.Abs(vn2 - cp2.velocityBias) < k_errorTol);
                        #endif
                        break;
                    }

                    //
                    // Case 4: x1 = 0 and x2 = 0
                    //
                    // vn1 = b1
                    // vn2 = b2;
                    x.X = 0.0f;
                    x.Y = 0.0f;
                    vn1 = b.X;
                    vn2 = b.Y;

                    if (vn1 >= 0.0f && vn2 >= 0.0f)
                    {
                        // Resubstitute for the incremental impulse
                        float dX = x.X - a.X;
                        float dY = x.Y - a.Y;

                        // Apply incremental impulse
                        P1X = dX * normalX;
                        P1Y = dX * normalY;
                        P2X = dY * normalX;
                        P2Y = dY * normalY;
                        vAX -= mA * (P1X + P2X);
                        vAY -= mA * (P1Y + P2Y);

                        // wA -= iA * (MathUtils.Cross(cp1.Ra, P1) + MathUtils.Cross(cp2.Ra, P2)); // inline
                        wA -= iA * (cp1RaX * P1Y - cp1RaY * P1X + (cp2RaX * P2Y - cp2RaY * P2X));

                        vBX += mB * (P1X + P2X);
                        vBY += mB * (P1Y + P2Y);

                        // wB += iB * (MathUtils.Cross(cp1.Rb, P1) + MathUtils.Cross(cp2.Rb, P2)); // inline
                        wB += iB * (cp1RbX * P1Y - cp1RbY * P1X + (cp2RbX * P2Y - cp2RbY * P2X));

                        // Accumulate
                        cp1NormalImpulse = x.X;
                        cp2NormalImpulse = x.Y;
                    }

                    // No solution, give up. This is hit sometimes, but it doesn't seem to matter.
                    break;
                }
            }

            velocities[indexA].V.X = vAX;
            velocities[indexA].V.Y = vAY;
            velocities[indexA].W = wA;
            velocities[indexB].V.X = vBX;
            velocities[indexB].V.Y = vBY;
            velocities[indexB].W = wB;
        }
    }

    public void StoreImpulses()
    {
        Span<ContactVelocityConstraint> velocityConstraints = this.VelocityConstraints;
        Span<Contact> contacts = this._contacts;
        for (int i = 0; i < this._contactCount; ++i)
        {
            ref ContactVelocityConstraint vc = ref velocityConstraints[i];
            ref Manifold manifold = ref contacts[vc.ContactIndex].Manifold;
            if (vc.PointCount == 1)
            {
                manifold.Points.Value0.NormalImpulse = vc.Points.Value0.NormalImpulse;
                manifold.Points.Value0.TangentImpulse = vc.Points.Value0.TangentImpulse;
                continue;
            }

            if (vc.PointCount == 2)
            {
                manifold.Points.Value0.NormalImpulse = vc.Points.Value0.NormalImpulse;
                manifold.Points.Value0.TangentImpulse = vc.Points.Value0.TangentImpulse;
                manifold.Points.Value1.NormalImpulse = vc.Points.Value1.NormalImpulse;
                manifold.Points.Value1.TangentImpulse = vc.Points.Value1.TangentImpulse;
            }
        }
    }

    public bool SolvePositionConstraints()
    {
        float minSeparation = 0.0f;
        Span<ContactPositionConstraint> positionConstraints = this.PositionConstraints;
        Span<Position> positions = this._positions;
        for (int i = 0; i < this._contactCount; ++i)
        {
            ref ContactPositionConstraint pc = ref positionConstraints[i];

            int indexA = pc.IndexA;
            int indexB = pc.IndexB;
            Vector2 localCenterA = pc.LocalCenterA;
            float mA = pc.InvMassA;
            float iA = pc.InvIa;
            Vector2 localCenterB = pc.LocalCenterB;
            float mB = pc.InvMassB;
            float iB = pc.InvIb;
            int pointCount = pc.PointCount;

            Vector2 cA = positions[indexA].Center;
            float aA = positions[indexA].Angle;

            Vector2 cB = positions[indexB].Center;
            float aB = positions[indexB].Angle;

            // Solve normal constraints
            for (int j = 0; j < pointCount; ++j)
            {
                Transform xfA = new Transform();
                Transform xfB = xfA;
                xfA.Rotation.Set(aA);
                xfB.Rotation.Set(aB);
                xfA.Position = cA - MathUtils.Mul(xfA.Rotation, localCenterA);
                xfB.Position = cB - MathUtils.Mul(xfB.Rotation, localCenterB);

                PositionSolverManifold psm = new PositionSolverManifold();
                psm.Initialize(pc, xfA, xfB, j);
                Vector2 normal = psm.Normal;

                Vector2 point = psm.Point;
                float separation = psm.Separation;

                Vector2 rA = point - cA;
                Vector2 rB = point - cB;

                // Track max constraint error.
                minSeparation = Math.Min(minSeparation, separation);

                // Prevent large corrections and allow slop.
                float C = MathUtils.Clamp(
                    Settings.Baumgarte * (separation + Settings.LinearSlop),
                    -Settings.MaxLinearCorrection,
                    0.0f);

                // Compute the effective mass.
                float rnA = MathUtils.Cross(rA, normal);
                float rnB = MathUtils.Cross(rB, normal);
                float K = mA + mB + iA * rnA * rnA + iB * rnB * rnB;

                // Compute normal impulse
                float impulse = K > 0.0f ? -C / K : 0.0f;

                Vector2 P = impulse * normal;

                cA -= mA * P;
                aA -= iA * MathUtils.Cross(rA, P);

                cB += mB * P;
                aB += iB * MathUtils.Cross(rB, P);
            }

            positions[indexA].Center = cA;
            positions[indexA].Angle = aA;

            positions[indexB].Center = cB;
            positions[indexB].Angle = aB;
        }

        // We can't expect minSpeparation >= -b2_linearSlop because we don't
        // push the separation above -b2_linearSlop.
        return minSeparation >= -3.0f * Settings.LinearSlop;
    }

    public bool SolveTOIPositionConstraints(int toiIndexA, int toiIndexB)
    {
        float minSeparation = 0.0f;
        Span<ContactPositionConstraint> positionConstraints = this.PositionConstraints;
        Span<Position> positions = this._positions;
        for (int i = 0; i < this._contactCount; ++i)
        {
            ref ContactPositionConstraint pc = ref positionConstraints[i];

            int indexA = pc.IndexA;
            int indexB = pc.IndexB;
            Vector2 localCenterA = pc.LocalCenterA;
            Vector2 localCenterB = pc.LocalCenterB;
            int pointCount = pc.PointCount;

            float mA = 0.0f;
            float iA = 0.0f;
            if (indexA == toiIndexA || indexA == toiIndexB)
            {
                mA = pc.InvMassA;
                iA = pc.InvIa;
            }

            float mB = 0.0f;
            float iB = 0.0f;
            if (indexB == toiIndexA || indexB == toiIndexB)
            {
                mB = pc.InvMassB;
                iB = pc.InvIb;
            }

            Vector2 cA = positions[indexA].Center;
            float aA = positions[indexA].Angle;

            Vector2 cB = positions[indexB].Center;
            float aB = positions[indexB].Angle;

            // Solve normal constraints
            for (int j = 0; j < pointCount; ++j)
            {
                Transform xfA = new Transform();
                Transform xfB = new Transform();
                xfA.Rotation.Set(aA);
                xfB.Rotation.Set(aB);
                xfA.Position = cA - MathUtils.Mul(xfA.Rotation, localCenterA);
                xfB.Position = cB - MathUtils.Mul(xfB.Rotation, localCenterB);

                PositionSolverManifold psm = new PositionSolverManifold();
                psm.Initialize(pc, xfA, xfB, j);
                Vector2 normal = psm.Normal;

                Vector2 point = psm.Point;
                float separation = psm.Separation;

                Vector2 rA = point - cA;
                Vector2 rB = point - cB;

                // Track max constraint error.
                minSeparation = Math.Min(minSeparation, separation);

                // Prevent large corrections and allow slop.
                float C = MathUtils.Clamp(
                    Settings.ToiBaumgarte * (separation + Settings.LinearSlop),
                    -Settings.MaxLinearCorrection,
                    0.0f);

                // Compute the effective mass.
                float rnA = MathUtils.Cross(rA, normal);
                float rnB = MathUtils.Cross(rB, normal);
                float K = mA + mB + iA * rnA * rnA + iB * rnB * rnB;

                // Compute normal impulse
                float impulse = K > 0.0f ? -C / K : 0.0f;

                Vector2 P = impulse * normal;

                cA -= mA * P;
                aA -= iA * MathUtils.Cross(rA, P);

                cB += mB * P;
                aB += iB * MathUtils.Cross(rB, P);
            }

            this._positions[indexA].Center = cA;
            this._positions[indexA].Angle = aA;

            this._positions[indexB].Center = cB;
            this._positions[indexB].Angle = aB;
        }

        // We can't expect minSpeparation >= -b2_linearSlop because we don't
        // push the separation above -b2_linearSlop.
        return minSeparation >= -1.5f * Settings.LinearSlop;
    }
}