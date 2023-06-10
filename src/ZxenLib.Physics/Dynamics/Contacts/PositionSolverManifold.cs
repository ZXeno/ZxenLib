namespace ZxenLib.Physics.Dynamics.Contacts;

using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Collision.Collider;
using Common;

public struct PositionSolverManifold
{
    public Vector2 Normal;

    public Vector2 Point;

    public float Separation;

    public void Initialize(in ContactPositionConstraint pc, in Transform xfA, in Transform xfB, int index)
    {
        Debug.Assert(pc.PointCount > 0);

        switch (pc.Type)
        {
            case ManifoldType.Circles:
            {
                //var pointA = MathUtils.Mul(xfA, pc.LocalPoint); // inline
                float x = xfA.Rotation.Cos * pc.LocalPoint.X - xfA.Rotation.Sin * pc.LocalPoint.Y + xfA.Position.X;
                float y = xfA.Rotation.Sin * pc.LocalPoint.X + xfA.Rotation.Cos * pc.LocalPoint.Y + xfA.Position.Y;
                Vector2 pointA = new Vector2(x, y);

                // var pointB = MathUtils.Mul(xfB, pc.LocalPoints.Value0); // inline
                x = xfB.Rotation.Cos * pc.LocalPoints.Value0.X - xfB.Rotation.Sin * pc.LocalPoints.Value0.Y + xfB.Position.X;
                y = xfB.Rotation.Sin * pc.LocalPoints.Value0.X + xfB.Rotation.Cos * pc.LocalPoints.Value0.Y + xfB.Position.Y;
                Vector2 pointB = new Vector2(x, y);

                this.Normal = pointB - pointA;
                this.Normal.Normalize();
                this.Point = 0.5f * (pointA + pointB);
                this.Separation = Vector2.Dot(pointB - pointA, this.Normal) - pc.RadiusA - pc.RadiusB;
            }
                break;

            case ManifoldType.FaceA:
            {
                // Normal = MathUtils.Mul(xfA.Rotation, pc.LocalNormal); // inline
                this.Normal = new Vector2(
                    xfA.Rotation.Cos * pc.LocalNormal.X - xfA.Rotation.Sin * pc.LocalNormal.Y,
                    xfA.Rotation.Sin * pc.LocalNormal.X + xfA.Rotation.Cos * pc.LocalNormal.Y);

                // var planePoint = MathUtils.Mul(xfA, pc.LocalPoint); // inline
                float x = xfA.Rotation.Cos * pc.LocalPoint.X - xfA.Rotation.Sin * pc.LocalPoint.Y + xfA.Position.X;
                float y = xfA.Rotation.Sin * pc.LocalPoint.X + xfA.Rotation.Cos * pc.LocalPoint.Y + xfA.Position.Y;
                Vector2 planePoint = new Vector2(x, y);

                // var clipPoint = MathUtils.Mul(xfB, pc.LocalPoints[index]); // inline

                if (index == 0)
                {
                    x = xfB.Rotation.Cos * pc.LocalPoints.Value0.X - xfB.Rotation.Sin * pc.LocalPoints.Value0.Y + xfB.Position.X;
                    y = xfB.Rotation.Sin * pc.LocalPoints.Value0.X + xfB.Rotation.Cos * pc.LocalPoints.Value0.Y + xfB.Position.Y;
                }
                else
                {
                    x = xfB.Rotation.Cos * pc.LocalPoints.Value1.X - xfB.Rotation.Sin * pc.LocalPoints.Value1.Y + xfB.Position.X;
                    y = xfB.Rotation.Sin * pc.LocalPoints.Value1.X + xfB.Rotation.Cos * pc.LocalPoints.Value1.Y + xfB.Position.Y;
                }

                Vector2 clipPoint = new Vector2(x, y);

                this.Separation = Vector2.Dot(clipPoint - planePoint, this.Normal) - pc.RadiusA - pc.RadiusB;
                this.Point = clipPoint;
            }
                break;

            case ManifoldType.FaceB:
            {
                // Normal = MathUtils.Mul(xfB.Rotation, pc.LocalNormal); // inline
                this.Normal = new Vector2(
                    xfB.Rotation.Cos * pc.LocalNormal.X - xfB.Rotation.Sin * pc.LocalNormal.Y,
                    xfB.Rotation.Sin * pc.LocalNormal.X + xfB.Rotation.Cos * pc.LocalNormal.Y);

                // var planePoint = MathUtils.Mul(xfB, pc.LocalPoint); // inline
                float x = xfB.Rotation.Cos * pc.LocalPoint.X - xfB.Rotation.Sin * pc.LocalPoint.Y + xfB.Position.X;
                float y = xfB.Rotation.Sin * pc.LocalPoint.X + xfB.Rotation.Cos * pc.LocalPoint.Y + xfB.Position.Y;
                Vector2 planePoint = new Vector2(x, y);

                // var clipPoint = MathUtils.Mul(xfA, pc.LocalPoints[index]); // inline
                if (index == 0)
                {
                    x = xfA.Rotation.Cos * pc.LocalPoints.Value0.X - xfA.Rotation.Sin * pc.LocalPoints.Value0.Y + xfA.Position.X;
                    y = xfA.Rotation.Sin * pc.LocalPoints.Value0.X + xfA.Rotation.Cos * pc.LocalPoints.Value0.Y + xfA.Position.Y;
                }
                else
                {
                    x = xfA.Rotation.Cos * pc.LocalPoints.Value1.X - xfA.Rotation.Sin * pc.LocalPoints.Value1.Y + xfA.Position.X;
                    y = xfA.Rotation.Sin * pc.LocalPoints.Value1.X + xfA.Rotation.Cos * pc.LocalPoints.Value1.Y + xfA.Position.Y;
                }

                Vector2 clipPoint = new Vector2(x, y);

                this.Separation = Vector2.Dot(clipPoint - planePoint, this.Normal) - pc.RadiusA - pc.RadiusB;
                this.Point = clipPoint;

                // Ensure normal points from A to B
                this.Normal = -this.Normal;
            }
                break;
            default:
                throw new InvalidEnumArgumentException($"Invalid ManifoldType: {pc.Type}");
        }
    }
}