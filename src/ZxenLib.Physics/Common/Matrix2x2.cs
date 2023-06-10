namespace ZxenLib.Physics.Common;

using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;

public struct Matrix2x2
{
    public Vector2 Ex;

    public Vector2 Ey;

    /// The default constructor does nothing (for performance).
    /// Construct this matrix using columns.
    public Matrix2x2(in Vector2 c1, in Vector2 c2)
    {
        this.Ex = c1;
        this.Ey = c2;
    }

    /// Construct this matrix using scalars.
    public Matrix2x2(float a11, float a12, float a21, float a22)
    {
        this.Ex.X = a11;
        this.Ex.Y = a21;
        this.Ey.X = a12;
        this.Ey.Y = a22;
    }

    /// Initialize this matrix using columns.
    public void Set(in Vector2 c1, in Vector2 c2)
    {
        this.Ex = c1;
        this.Ey = c2;
    }

    /// Set this to the identity matrix.
    public void SetIdentity()
    {
        this.Ex.X = 1.0f;
        this.Ey.X = 0.0f;
        this.Ex.Y = 0.0f;
        this.Ey.Y = 1.0f;
    }

    /// Set this matrix to all zeros.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetZero()
    {
        this.Ex.X = 0.0f;
        this.Ey.X = 0.0f;
        this.Ex.Y = 0.0f;
        this.Ey.Y = 0.0f;
    }

    public Matrix2x2 GetInverse()
    {
        float a = this.Ex.X;
        float b = this.Ey.X;
        float c = this.Ex.Y;
        float d = this.Ey.Y;

        float det = a * d - b * c;
        if (!det.Equals(0.0f))
        {
            det = 1.0f / det;
        }

        Matrix2x2 B = new Matrix2x2();
        B.Ex.X = det * d;
        B.Ey.X = -det * b;
        B.Ex.Y = -det * c;
        B.Ey.Y = det * a;
        return B;
    }

    /// Solve A * x = b, where b is a column vector. This is more efficient
    /// than computing the inverse in one-shot cases.
    public Vector2 Solve(in Vector2 b)
    {
        float a11 = this.Ex.X;
        float a12 = this.Ey.X;
        float a21 = this.Ex.Y;
        float a22 = this.Ey.Y;
        float det = a11 * a22 - a12 * a21;
        if (!det.Equals(0.0f))
        {
            det = 1.0f / det;
        }

        Vector2 x = new Vector2 {X = det * (a22 * b.X - a12 * b.Y), Y = det * (a11 * b.Y - a21 * b.X)};
        return x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix2x2 operator +(in Matrix2x2 A, in Matrix2x2 B)
    {
        return new Matrix2x2(A.Ex + B.Ex, A.Ey + B.Ey);
    }
}