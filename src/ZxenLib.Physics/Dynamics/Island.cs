namespace ZxenLib.Physics.Dynamics;

using System;
using System.Buffers;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Common;
using Contacts;
using Joints;

/// This is an internal class.
public class Island
{
    internal Body[] Bodies;

    internal int BodyCount;

    internal int ContactCount;

    internal IContactListener ContactListener;

    internal Contact[] Contacts;

    internal int JointCount;

    internal Joint[] Joints;

    internal Position[] Positions;

    internal Velocity[] Velocities;

    public void Setup(
        int bodyCapacity,
        int contactCapacity,
        int jointCapacity,
        IContactListener contactListener)
    {
        this.BodyCount = 0;
        this.ContactCount = 0;
        this.JointCount = 0;

        this.ContactListener = contactListener;

        this.Bodies = ArrayPool<Body>.Shared.Rent(bodyCapacity);
        this.Contacts = ArrayPool<Contact>.Shared.Rent(contactCapacity);
        this.Joints = ArrayPool<Joint>.Shared.Rent(jointCapacity);
        this.Positions = ArrayPool<Position>.Shared.Rent(bodyCapacity);
        this.Velocities = ArrayPool<Velocity>.Shared.Rent(bodyCapacity);
    }

    internal void Reset()
    {
        this.BodyCount = 0;
        this.ContactCount = 0;
        this.JointCount = 0;

        ArrayPool<Body>.Shared.Return(this.Bodies, true);
        this.Bodies = default;

        ArrayPool<Contact>.Shared.Return(this.Contacts, true);
        this.Contacts = default;

        ArrayPool<Joint>.Shared.Return(this.Joints, true);
        this.Joints = default;

        ArrayPool<Position>.Shared.Return(this.Positions, true);
        this.Positions = default;

        ArrayPool<Velocity>.Shared.Return(this.Velocities, true);
        this.Velocities = default;
    }

    ~Island()
    {
        if (this.Bodies != null)
        {
            ArrayPool<Body>.Shared.Return(this.Bodies, true);
        }

        if (this.Contacts != null)
        {
            ArrayPool<Contact>.Shared.Return(this.Contacts, true);
        }

        if (this.Joints != null)
        {
            ArrayPool<Joint>.Shared.Return(this.Joints, true);
        }

        if (this.Positions != null)
        {
            ArrayPool<Position>.Shared.Return(this.Positions, true);
        }

        if (this.Velocities != null)
        {
            ArrayPool<Velocity>.Shared.Return(this.Velocities, true);
        }
    }

    internal void Clear()
    {
        this.BodyCount = 0;
        this.ContactCount = 0;
        this.JointCount = 0;
    }

    private readonly ContactSolver _solveContactSolver = new ContactSolver();

    private readonly Stopwatch _solveTimer = new Stopwatch();

    internal void Solve(out Profile profile, in TimeStep step, in Vector2 gravity, bool allowSleep)
    {
        profile = default;

        float h = step.Dt;

        // Integrate velocities and apply damping. Initialize the body state.
        for (int i = 0; i < this.BodyCount; ++i)
        {
            Body? b = this.Bodies[i];

            Vector2 c = b.Sweep.C;
            float a = b.Sweep.A;
            Vector2 v = b.LinearVelocity;
            float w = b.AngularVelocity;

            // Store positions for continuous collision.
            b.Sweep.C0 = b.Sweep.C;
            b.Sweep.A0 = b.Sweep.A;

            if (b.BodyType == BodyType.DynamicBody)
            {
                // Integrate velocities.
                v += h * b.InvMass * (b.GravityScale * b.Mass * gravity + b.Force);
                w += h * b.InverseInertia * b.Torque;

                // Apply damping.
                // ODE: dv/dt + c * v = 0
                // Solution: v(t) = v0 * exp(-c * t)
                // Time step: v(t + dt) = v0 * exp(-c * (t + dt)) = v0 * exp(-c * t) * exp(-c * dt) = v * exp(-c * dt)
                // v2 = exp(-c * dt) * v1
                // Pade approximation:
                // v2 = v1 * 1 / (1 + c * dt)
                v *= 1.0f / (1.0f + h * b.LinearDamping);
                w *= 1.0f / (1.0f + h * b.AngularDamping);
            }

            this.Positions[i].Center = c;
            this.Positions[i].Angle = a;
            this.Velocities[i].V = v;
            this.Velocities[i].W = w;
        }

        this._solveTimer.Restart();

        // Solver data
        SolverData solverData = new SolverData(in step, this.Positions, this.Velocities);

        // Initialize velocity constraints.
        ContactSolverDef contactSolverDef = new ContactSolverDef(in step, this.ContactCount, this.Contacts, this.Positions, this.Velocities);

        ContactSolver? contactSolver = this._solveContactSolver;
        contactSolver.Setup(in contactSolverDef);
        contactSolver.InitializeVelocityConstraints();

        if (step.WarmStarting)
        {
            contactSolver.WarmStart();
        }

        for (int i = 0; i < this.JointCount; ++i)
        {
            this.Joints[i].InitVelocityConstraints(in solverData);
        }

        profile.SolveInit = this._solveTimer.ElapsedMilliseconds;

        // Solve velocity constraints
        this._solveTimer.Restart();
        for (int i = 0; i < step.VelocityIterations; ++i)
        {
            for (int j = 0; j < this.JointCount; ++j)
            {
                this.Joints[j].SolveVelocityConstraints(in solverData);
            }

            contactSolver.SolveVelocityConstraints();
        }

        // Store impulses for warm starting
        contactSolver.StoreImpulses();
        this._solveTimer.Stop();
        profile.SolveVelocity = this._solveTimer.ElapsedMilliseconds;

        // Integrate positions
        for (int i = 0; i < this.BodyCount; ++i)
        {
            Vector2 c = this.Positions[i].Center;
            float a = this.Positions[i].Angle;
            Vector2 v = this.Velocities[i].V;
            float w = this.Velocities[i].W;

            // Check for large velocities
            Vector2 translation = h * v;
            if (Vector2.Dot(translation, translation) > Settings.MaxTranslationSquared)
            {
                float ratio = Settings.MaxTranslation / translation.Length();
                v *= ratio;
            }

            float rotation = h * w;
            if (rotation * rotation > Settings.MaxRotationSquared)
            {
                float ratio = Settings.MaxRotation / Math.Abs(rotation);
                w *= ratio;
            }

            // Integrate
            c += h * v;
            a += h * w;

            this.Positions[i].Center = c;
            this.Positions[i].Angle = a;
            this.Velocities[i].V = v;
            this.Velocities[i].W = w;
        }

        // Solve position constraints
        this._solveTimer.Restart();
        bool positionSolved = false;
        for (int i = 0; i < step.PositionIterations; ++i)
        {
            bool contactsOkay = contactSolver.SolvePositionConstraints();

            bool jointsOkay = true;
            for (int j = 0; j < this.JointCount; ++j)
            {
                bool jointOkay = this.Joints[j].SolvePositionConstraints(in solverData);
                jointsOkay = jointsOkay && jointOkay;
            }

            if (contactsOkay && jointsOkay)
            {
                // Exit early if the position errors are small.
                positionSolved = true;
                break;
            }
        }

        // Copy state buffers back to the bodies
        for (int i = 0; i < this.BodyCount; ++i)
        {
            Body? body = this.Bodies[i];
            body.Sweep.C = this.Positions[i].Center;
            body.Sweep.A = this.Positions[i].Angle;
            body.LinearVelocity = this.Velocities[i].V;
            body.AngularVelocity = this.Velocities[i].W;
            body.SynchronizeTransform();
        }

        this._solveTimer.Stop();
        profile.SolvePosition = this._solveTimer.ElapsedMilliseconds;

        this.Report(contactSolver.VelocityConstraints);

        if (allowSleep)
        {
            float minSleepTime = Settings.MaxFloat;

            // 线速度最小值平方
            const float linTolSqr = Settings.LinearSleepTolerance * Settings.LinearSleepTolerance;

            // 角速度最小值平方
            const float angTolSqr = Settings.AngularSleepTolerance * Settings.AngularSleepTolerance;

            for (int i = 0; i < this.BodyCount; ++i)
            {
                Body? b = this.Bodies[i];
                if (b.BodyType == BodyType.StaticBody) // 静态物体没有休眠
                {
                    continue;
                }

                if (!b.Flags.IsSet(BodyFlags.AutoSleep)                              // 不允许休眠
                    || b.AngularVelocity * b.AngularVelocity > angTolSqr            // 或 角速度大于最小值
                    || Vector2.Dot(b.LinearVelocity, b.LinearVelocity) > linTolSqr) // 或 线速度大于最小值
                {
                    b.SleepTime = 0.0f;
                    minSleepTime = 0.0f;
                }
                else
                {
                    b.SleepTime += h;
                    minSleepTime = Math.Min(minSleepTime, b.SleepTime);
                }
            }

            if (minSleepTime >= Settings.TimeToSleep && positionSolved)
            {
                for (int i = 0; i < this.BodyCount; ++i)
                {
                    Body? b = this.Bodies[i];
                    b.IsAwake = false;
                }
            }
        }

        contactSolver.Reset();
    }

    private readonly ContactSolver _solveToiContactSolver = new ContactSolver();

    internal void SolveTOI(in TimeStep subStep, int toiIndexA, int toiIndexB)
    {
        Debug.Assert(toiIndexA < this.BodyCount);
        Debug.Assert(toiIndexB < this.BodyCount);

        // Initialize the body state.
        for (int i = 0; i < this.BodyCount; ++i)
        {
            Body? b = this.Bodies[i];
            this.Positions[i].Center = b.Sweep.C;
            this.Positions[i].Angle = b.Sweep.A;
            this.Velocities[i].V = b.LinearVelocity;
            this.Velocities[i].W = b.AngularVelocity;
        }

        ContactSolverDef contactSolverDef = new ContactSolverDef(in subStep, this.ContactCount, this.Contacts, this.Positions, this.Velocities);
        ContactSolver? contactSolver = this._solveToiContactSolver;
        contactSolver.Setup(in contactSolverDef);

        // Solve position constraints.
        for (int i = 0; i < subStep.PositionIterations; ++i)
        {
            bool contactsOkay = contactSolver.SolveTOIPositionConstraints(toiIndexA, toiIndexB);
            if (contactsOkay)
            {
                break;
            }
        }

        #if FALSE
// Is the new position really safe?
            for (int i = 0; i < m_contactCount; ++i)
            {
                var c = m_contacts[i];
                var fA = c.FixtureA;
                var fB = c.FixtureB;

                var bA = fA.GetBody();
                var bB = fB.GetBody();

                int indexA = c.GetChildIndexA();
                int indexB = c.GetChildIndexB();

                b2DistanceInput input = new b2DistanceInput();
                input.proxyA.Set(fA.Shape, indexA);
                input.proxyB.Set(fB.Shape, indexB);
                input.transformA = bA.GetTransform();
                input.transformB = bB.GetTransform();
                input.useRadii = false;

                b2DistanceOutput output = new b2DistanceOutput();
                SimplexCache     cache = new SimplexCache {count = 0};
                DistanceAlgorithm.b2Distance(ref output, ref cache, ref input);

                if (output.distance.Equals(0) || cache.count == 3)
                {
                    cache.count += 0;
                }
            }
        #endif

        // Leap of faith to new safe state.
        this.Bodies[toiIndexA].Sweep.C0 = this.Positions[toiIndexA].Center;
        this.Bodies[toiIndexA].Sweep.A0 = this.Positions[toiIndexA].Angle;
        this.Bodies[toiIndexB].Sweep.C0 = this.Positions[toiIndexB].Center;
        this.Bodies[toiIndexB].Sweep.A0 = this.Positions[toiIndexB].Angle;

        // No warm starting is needed for TOI events because warm
        // starting impulses were applied in the discrete solver.
        contactSolver.InitializeVelocityConstraints();

        // Solve velocity constraints.
        for (int i = 0; i < subStep.VelocityIterations; ++i)
        {
            contactSolver.SolveVelocityConstraints();
        }

        // Don't store the TOI contact forces for warm starting
        // because they can be quite large.

        float h = subStep.Dt;

        // Integrate positions
        for (int i = 0; i < this.BodyCount; ++i)
        {
            Vector2 c = this.Positions[i].Center;
            float a = this.Positions[i].Angle;
            Vector2 v = this.Velocities[i].V;
            float w = this.Velocities[i].W;

            // Check for large velocities
            Vector2 translation = h * v;
            if (Vector2.Dot(translation, translation) > Settings.MaxTranslationSquared)
            {
                float ratio = Settings.MaxTranslation / translation.Length();
                v *= ratio;
            }

            float rotation = h * w;
            if (rotation * rotation > Settings.MaxRotationSquared)
            {
                float ratio = Settings.MaxRotation / Math.Abs(rotation);
                w *= ratio;
            }

            // Integrate
            c += h * v;
            a += h * w;

            this.Positions[i].Center = c;
            this.Positions[i].Angle = a;
            this.Velocities[i].V = v;
            this.Velocities[i].W = w;

            // Sync bodies
            Body? body = this.Bodies[i];
            body.Sweep.C = c;
            body.Sweep.A = a;
            body.LinearVelocity = v;
            body.AngularVelocity = w;
            body.SynchronizeTransform();
        }

        this.Report(contactSolver.VelocityConstraints);
        contactSolver.Reset();
    }

    internal void Add(Body body)
    {
        Debug.Assert(this.BodyCount < this.Bodies.Length);
        body.IslandIndex = this.BodyCount;
        this.Bodies[this.BodyCount] = body;
        ++this.BodyCount;
    }

    internal void Add(Contact contact)
    {
        Debug.Assert(this.ContactCount < this.Contacts.Length);
        this.Contacts[this.ContactCount++] = contact;
    }

    internal void Add(Joint joint)
    {
        Debug.Assert(this.JointCount < this.Joints.Length);
        this.Joints[this.JointCount++] = joint;
    }

    private void Report(ContactVelocityConstraint[] constraints)
    {
        if (this.ContactListener == null)
        {
            return;
        }

        for (int i = 0; i < this.ContactCount; ++i)
        {
            Contact? c = this.Contacts[i];

            ContactVelocityConstraint vc = constraints[i];

            ContactImpulse impulse = new ContactImpulse {Count = vc.PointCount};
            for (int j = 0; j < vc.PointCount; ++j)
            {
                impulse.NormalImpulses[j] = vc.Points[j].NormalImpulse;
                impulse.TangentImpulses[j] = vc.Points[j].TangentImpulse;
            }

            this.ContactListener.PostSolve(c, impulse);
        }
    }
}