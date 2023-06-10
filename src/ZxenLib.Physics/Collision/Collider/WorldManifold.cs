namespace ZxenLib.Physics.Collision.Collider;

using Microsoft.Xna.Framework;
using Common;

/// This is used to compute the current state of a contact manifold.
public struct WorldManifold
{
    /// Evaluate the manifold with supplied transforms. This assumes
    /// modest motion from the original state. This does not change the
    /// point count, impulses, etc. The radii must come from the shapes
    /// that generated the manifold.
    public void Initialize(
        in Manifold manifold,
        in Transform xfA,
        float radiusA,
        in Transform xfB,
        float radiusB)
    {
        if (manifold.PointCount == 0)
        {
            return;
        }

        switch (manifold.Type)
        {
            case ManifoldType.Circles:
            {
                this.Normal.Set(1.0f, 0.0f);
                Vector2 pointA = MathUtils.Mul(xfA, manifold.LocalPoint);
                Vector2 pointB = MathUtils.Mul(xfB, manifold.Points.Value0.LocalPoint);
                if (Vector2.DistanceSquared(pointA, pointB) > Settings.Epsilon * Settings.Epsilon)
                {
                    this.Normal = pointB - pointA;
                    this.Normal.Normalize();
                }

                Vector2 cA = pointA + radiusA * this.Normal;
                Vector2 cB = pointB - radiusB * this.Normal;
                this.Points.Value0 = 0.5f * (cA + cB);
                this.Separations.Value0 = Vector2.Dot(cB - cA, this.Normal);
            }
                break;

            case ManifoldType.FaceA:
            {
                this.Normal = MathUtils.Mul(xfA.Rotation, manifold.LocalNormal);
                Vector2 planePoint = MathUtils.Mul(xfA, manifold.LocalPoint);

                for (int i = 0; i < manifold.PointCount; ++i)
                {
                    Vector2 clipPoint = MathUtils.Mul(xfB, manifold.Points[i].LocalPoint);
                    Vector2 cA = clipPoint + (radiusA - Vector2.Dot(clipPoint - planePoint, this.Normal)) * this.Normal;
                    Vector2 cB = clipPoint - radiusB * this.Normal;
                    this.Points[i] = 0.5f * (cA + cB);
                    this.Separations[i] = Vector2.Dot(cB - cA, this.Normal);
                }
            }
                break;

            case ManifoldType.FaceB:
            {
                this.Normal = MathUtils.Mul(xfB.Rotation, manifold.LocalNormal);
                Vector2 planePoint = MathUtils.Mul(xfB, manifold.LocalPoint);

                for (int i = 0; i < manifold.PointCount; ++i)
                {
                    Vector2 clipPoint = MathUtils.Mul(xfA, manifold.Points[i].LocalPoint);
                    Vector2 cB = clipPoint + (radiusB - Vector2.Dot(clipPoint - planePoint, this.Normal)) * this.Normal;
                    Vector2 cA = clipPoint - radiusA * this.Normal;
                    this.Points[i] = 0.5f * (cA + cB);
                    this.Separations[i] = Vector2.Dot(cA - cB, this.Normal);
                }

                // Ensure normal points from A to B.
                this.Normal = -this.Normal;
            }
                break;
        }
    }

    /// world vector pointing from A to B
    public Vector2 Normal;

    /// <summary>
    /// world contact point (point of intersection), size Settings.MaxManifoldPoints
    /// </summary>
    public FixedArray2<Vector2> Points;

    /// <summary>
    /// a negative value indicates overlap, in meters, size Settings.MaxManifoldPoints
    /// </summary>
    public FixedArray2<float> Separations;
}