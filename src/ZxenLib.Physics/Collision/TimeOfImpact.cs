namespace ZxenLib.Physics.Collision;

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Common;
using Dynamics;
using Shapes;

/// Input parameters for b2TimeOfImpact
public struct ToiInput
{
    public DistanceProxy ProxyA;

    public DistanceProxy ProxyB;

    public Sweep SweepA;

    public Sweep SweepB;

    public float Tmax; // defines sweep interval [0, tMax]
}

/// Output parameters for b2TimeOfImpact.
public struct ToiOutput
{
    public enum ToiState
    {
        Unknown,

        Failed,

        Overlapped,

        Touching,

        Separated
    }

    public ToiState State;

    public float Time;
}

public class ToiProfile
{
    public float ToiTime;

    public float ToiMaxTime;

    public int ToiCalls;

    public int ToiIters;

    public int ToiMaxIters;

    public int ToiRootIters;

    public int ToiMaxRootIters;

    public World World;
}

public static class TimeOfImpact
{
    /// Compute the upper bound on time before two shapes penetrate. Time is represented as
    /// a fraction between [0,tMax]. This uses a swept separating axis and may miss some intermediate,
    /// non-tunneling collisions. If you change the time interval, you should call this function
    /// again.
    /// Note: use b2Distance to compute the contact point and normal at the time of impact.
    public static void ComputeTimeOfImpact(out ToiOutput output, in ToiInput input, ToiProfile toiProfile = null, GJkProfile gjkProfile = null)
    {
        long beginTime = toiProfile == null ? 0 : Stopwatch.GetTimestamp();
        output = new ToiOutput();

        if (toiProfile != null)
        {
            ++toiProfile.ToiCalls;
        }

        output.State = ToiOutput.ToiState.Unknown;
        output.Time = input.Tmax;

        ref readonly DistanceProxy proxyA = ref input.ProxyA;
        ref readonly DistanceProxy proxyB = ref input.ProxyB;

        Sweep sweepA = input.SweepA;
        Sweep sweepB = input.SweepB;

        // Large rotations can make the root finder fail, so we normalize the
        // sweep angles.
        sweepA.Normalize();
        sweepB.Normalize();

        float tMax = input.Tmax;

        float totalRadius = proxyA.Radius + proxyB.Radius;
        float target = Math.Max(Settings.LinearSlop, totalRadius - 3.0f * Settings.LinearSlop);
        float tolerance = 0.25f * Settings.LinearSlop;
        Debug.Assert(target > tolerance);

        float t1 = 0.0f;
        const int maxIterations = 20; // TODO_ERIN b2Settings
        int iter = 0;

        // Prepare input for distance query.
        SimplexCache cache = new SimplexCache();
        DistanceInput distanceInput = new DistanceInput
        {
            ProxyA = input.ProxyA,
            ProxyB = input.ProxyB,
            UseRadii = false
        };

        // The outer loop progressively attempts to compute new separating axes.
        // This loop terminates when an axis is repeated (no progress is made).
        for (;;)
        {
            sweepA.GetTransform(out Transform xfA, t1);
            sweepB.GetTransform(out Transform xfB, t1);

            // Get the distance between shapes. We can also use the results
            // to get a separating axis.
            distanceInput.TransformA = xfA;
            distanceInput.TransformB = xfB;

            DistanceAlgorithm.Distance(out DistanceOutput distanceOutput, ref cache, distanceInput, gjkProfile);

            // If the shapes are overlapped, we give up on continuous collision.
            if (distanceOutput.Distance <= 0.0f)
            {
                // Failure!
                output.State = ToiOutput.ToiState.Overlapped;
                output.Time = 0.0f;
                break;
            }

            if (distanceOutput.Distance < target + tolerance)
            {
                // Victory!
                output.State = ToiOutput.ToiState.Touching;
                output.Time = t1;
                break;
            }

            // Initialize the separating axis.
            SeparationFunction fcn = new SeparationFunction();
            fcn.Initialize(ref cache, proxyA, sweepA, proxyB, sweepB, t1);

            // Compute the TOI on the separating axis. We do this by successively
            // resolving the deepest point. This loop is bounded by the number of vertices.
            bool done = false;
            float t2 = tMax;
            int pushBackIter = 0;
            for (;;)
            {
                // Find the deepest point at t2. Store the witness point indices.

                float s2 = fcn.FindMinSeparation(out int indexA, out int indexB, t2);

                // Is the final configuration separated?
                if (s2 > target + tolerance)
                {
                    // Victory!
                    output.State = ToiOutput.ToiState.Separated;
                    output.Time = tMax;
                    done = true;
                    break;
                }

                // Has the separation reached tolerance?
                if (s2 > target - tolerance)
                {
                    // Advance the sweeps
                    t1 = t2;
                    break;
                }

                // Compute the initial separation of the witness points.
                float s1 = fcn.Evaluate(indexA, indexB, t1);

                // Check for initial overlap. This might happen if the root finder
                // runs out of iterations.
                if (s1 < target - tolerance)
                {
                    output.State = ToiOutput.ToiState.Failed;
                    output.Time = t1;
                    done = true;
                    break;
                }

                // Check for touching
                if (s1 <= target + tolerance)
                {
                    // Victory! t1 should hold the TOI (could be 0.0).
                    output.State = ToiOutput.ToiState.Touching;
                    output.Time = t1;
                    done = true;
                    break;
                }

                // Compute 1D root of: f(x) - target = 0
                int rootIterCount = 0;
                float a1 = t1, a2 = t2;
                for (;;)
                {
                    // Use a mix of the secant rule and bisection.
                    float t;
                    if ((rootIterCount & 1) != 0)
                    {
                        // Secant rule to improve convergence.
                        t = a1 + (target - s1) * (a2 - a1) / (s2 - s1);
                    }
                    else
                    {
                        // Bisection to guarantee progress.
                        t = 0.5f * (a1 + a2);
                    }

                    ++rootIterCount;
                    if (toiProfile != null)
                    {
                        ++toiProfile.ToiRootIters;
                    }

                    float s = fcn.Evaluate(indexA, indexB, t);

                    if (Math.Abs(s - target) < tolerance)
                    {
                        // t2 holds a tentative value for t1
                        t2 = t;
                        break;
                    }

                    // Ensure we continue to bracket the root.
                    if (s > target)
                    {
                        a1 = t;
                        s1 = s;
                    }
                    else
                    {
                        a2 = t;
                        s2 = s;
                    }

                    if (rootIterCount == 50)
                    {
                        break;
                    }
                }

                if (toiProfile != null)
                {
                    toiProfile.ToiMaxRootIters = Math.Max(toiProfile.ToiMaxRootIters, rootIterCount);
                }

                ++pushBackIter;

                if (pushBackIter == Settings.MaxPolygonVertices)
                {
                    break;
                }
            }

            ++iter;
            if (toiProfile != null)
            {
                ++toiProfile.ToiIters;
            }

            if (done)
            {
                break;
            }

            if (iter == maxIterations)
            {
                // Root finder got stuck. Semi-victory.
                output.State = ToiOutput.ToiState.Failed;
                output.Time = t1;
                break;
            }
        }

        if (toiProfile == null)
        {
            return;
        }

        long endTime = Stopwatch.GetTimestamp();
        float time = (endTime - beginTime) / 10000f;
        toiProfile.ToiMaxIters = Math.Max(toiProfile.ToiMaxIters, iter);
        toiProfile.ToiMaxTime = Math.Max(toiProfile.ToiMaxTime, time);
        toiProfile.ToiTime += time;
    }
}

//
public struct SeparationFunction
{
    public enum FunctionType
    {
        Points,

        FaceA,

        FaceB
    }

    public Vector2 Axis;

    public Vector2 LocalPoint;

    public DistanceProxy ProxyA;

    public DistanceProxy ProxyB;

    public Sweep SweepA;

    public Sweep SweepB;

    public FunctionType Type;

    // TODO_ERIN might not need to return the separation

    public float Initialize(
        ref SimplexCache cache,
        DistanceProxy proxyA,
        in Sweep sweepA,
        DistanceProxy proxyB,
        in Sweep sweepB,
        float t1)
    {
        this.ProxyA = proxyA;
        this.ProxyB = proxyB;
        int count = cache.Count;
        Debug.Assert(0 < count && count < 3);
        byte av0 = cache.IndexA.Value0;
        byte av1 = cache.IndexA.Value1;
        byte bv0 = cache.IndexB.Value0;
        byte bv1 = cache.IndexB.Value1;
        this.SweepA = sweepA;
        this.SweepB = sweepB;

        this.SweepA.GetTransform(out Transform xfA, t1);
        this.SweepB.GetTransform(out Transform xfB, t1);

        if (count == 1)
        {
            this.Type = FunctionType.Points;
            Vector2 localPointA = this.ProxyA.GetVertex(av0);
            Vector2 localPointB = this.ProxyB.GetVertex(bv0);
            Vector2 pointA = MathUtils.Mul(xfA, localPointA);
            Vector2 pointB = MathUtils.Mul(xfB, localPointB);
            this.Axis = pointB - pointA;
            float s = MathExtensions.Normalize(ref this.Axis);
            return s;
        }

        if (av0 == av1)
        {
            // Two points on B and one on A.
            this.Type = FunctionType.FaceB;
            Vector2 localPointB1 = proxyB.GetVertex(bv0);
            Vector2 localPointB2 = proxyB.GetVertex(bv1);

            this.Axis = MathUtils.Cross(localPointB2 - localPointB1, 1.0f);
            this.Axis.Normalize();
            Vector2 normal = MathUtils.Mul(xfB.Rotation, this.Axis);

            this.LocalPoint = 0.5f * (localPointB1 + localPointB2);
            Vector2 pointB = MathUtils.Mul(xfB, this.LocalPoint);

            Vector2 localPointA = proxyA.GetVertex(av0);
            Vector2 pointA = MathUtils.Mul(xfA, localPointA);

            float s = Vector2.Dot(pointA - pointB, normal);
            if (s < 0.0f)
            {
                this.Axis = -this.Axis;
                s = -s;
            }

            return s;
        }
        else
        {
            // Two points on A and one or two points on B.
            this.Type = FunctionType.FaceA;
            Vector2 localPointA1 = this.ProxyA.GetVertex(av0);
            Vector2 localPointA2 = this.ProxyA.GetVertex(av1);

            this.Axis = MathUtils.Cross(localPointA2 - localPointA1, 1.0f);
            this.Axis.Normalize();
            Vector2 normal = MathUtils.Mul(xfA.Rotation, this.Axis);

            this.LocalPoint = 0.5f * (localPointA1 + localPointA2);
            Vector2 pointA = MathUtils.Mul(xfA, this.LocalPoint);

            Vector2 localPointB = this.ProxyB.GetVertex(bv0);
            Vector2 pointB = MathUtils.Mul(xfB, localPointB);

            float s = Vector2.Dot(pointB - pointA, normal);
            if (s < 0.0f)
            {
                this.Axis = -this.Axis;
                s = -s;
            }

            return s;
        }
    }

    //
    public float FindMinSeparation(out int indexA, out int indexB, float t)
    {
        this.SweepA.GetTransform(out Transform xfA, t);
        this.SweepB.GetTransform(out Transform xfB, t);

        switch (this.Type)
        {
            case FunctionType.Points:
            {
                Vector2 axisA = MathUtils.MulT(xfA.Rotation, this.Axis);
                Vector2 axisB = MathUtils.MulT(xfB.Rotation, -this.Axis);

                indexA = this.ProxyA.GetSupport(axisA);
                indexB = this.ProxyB.GetSupport(axisB);

                Vector2 localPointA = this.ProxyA.GetVertex(indexA);
                Vector2 localPointB = this.ProxyB.GetVertex(indexB);

                Vector2 pointA = MathUtils.Mul(xfA, localPointA);
                Vector2 pointB = MathUtils.Mul(xfB, localPointB);

                float separation = Vector2.Dot(pointB - pointA, this.Axis);
                return separation;
            }

            case FunctionType.FaceA:
            {
                Vector2 normal = MathUtils.Mul(xfA.Rotation, this.Axis);
                Vector2 pointA = MathUtils.Mul(xfA, this.LocalPoint);

                Vector2 axisB = MathUtils.MulT(xfB.Rotation, -normal);

                indexA = -1;
                indexB = this.ProxyB.GetSupport(axisB);

                Vector2 localPointB = this.ProxyB.GetVertex(indexB);
                Vector2 pointB = MathUtils.Mul(xfB, localPointB);

                float separation = Vector2.Dot(pointB - pointA, normal);
                return separation;
            }

            case FunctionType.FaceB:
            {
                Vector2 normal = MathUtils.Mul(xfB.Rotation, this.Axis);
                Vector2 pointB = MathUtils.Mul(xfB, this.LocalPoint);

                Vector2 axisA = MathUtils.MulT(xfA.Rotation, -normal);

                indexB = -1;
                indexA = this.ProxyA.GetSupport(axisA);

                Vector2 localPointA = this.ProxyA.GetVertex(indexA);
                Vector2 pointA = MathUtils.Mul(xfA, localPointA);

                float separation = Vector2.Dot(pointA - pointB, normal);
                return separation;
            }

            default:
                Debug.Assert(false);
                indexA = -1;
                indexB = -1;
                return 0.0f;
        }
    }

    //
    public float Evaluate(int indexA, int indexB, float t)
    {
        this.SweepA.GetTransform(out Transform xfA, t);
        this.SweepB.GetTransform(out Transform xfB, t);

        switch (this.Type)
        {
            case FunctionType.Points:
            {
                Vector2 localPointA = this.ProxyA.GetVertex(indexA);
                Vector2 localPointB = this.ProxyB.GetVertex(indexB);

                Vector2 pointA = MathUtils.Mul(xfA, localPointA);
                Vector2 pointB = MathUtils.Mul(xfB, localPointB);
                float separation = Vector2.Dot(pointB - pointA, this.Axis);

                return separation;
            }

            case FunctionType.FaceA:
            {
                Vector2 normal = MathUtils.Mul(xfA.Rotation, this.Axis);
                Vector2 pointA = MathUtils.Mul(xfA, this.LocalPoint);

                Vector2 localPointB = this.ProxyB.GetVertex(indexB);
                Vector2 pointB = MathUtils.Mul(xfB, localPointB);

                float separation = Vector2.Dot(pointB - pointA, normal);
                return separation;
            }

            case FunctionType.FaceB:
            {
                Vector2 normal = MathUtils.Mul(xfB.Rotation, this.Axis);
                Vector2 pointB = MathUtils.Mul(xfB, this.LocalPoint);

                Vector2 localPointA = this.ProxyA.GetVertex(indexA);
                Vector2 pointA = MathUtils.Mul(xfA, localPointA);

                float separation = Vector2.Dot(pointA - pointB, normal);
                return separation;
            }

            default:
                Debug.Assert(false);
                return 0.0f;
        }
    }
}