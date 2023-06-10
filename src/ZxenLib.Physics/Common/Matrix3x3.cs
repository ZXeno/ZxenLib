namespace ZxenLib.Physics.Common;

using Microsoft.Xna.Framework;

public struct Matrix3x3
{
    public Vector3 Ex;

    public Vector3 Ey;

    public Vector3 Ez;

    /// Construct this matrix using columns.
    public Matrix3x3(in Vector3 c1, in Vector3 c2, in Vector3 c3)
    {
        this.Ex = c1;
        this.Ey = c2;
        this.Ez = c3;
    }

    /// Set this matrix to all zeros.
    public void SetZero()
    {
        this.Ex.SetZero();
        this.Ey.SetZero();
        this.Ez.SetZero();
    }

    /// Solve A * x = b, where b is a column vector. This is more efficient
    /// than computing the inverse in one-shot cases.
    public Vector3 Solve33(in Vector3 b)
    {
        float det = Vector3.Dot(this.Ex, Vector3.Cross(this.Ey, this.Ez));
        if (!det.Equals(0.0f))
        {
            det = 1.0f / det;
        }

        Vector3 x;
        x.X = det * Vector3.Dot(b, Vector3.Cross(this.Ey, this.Ez));
        x.Y = det * Vector3.Dot(this.Ex, Vector3.Cross(b, this.Ez));
        x.Z = det * Vector3.Dot(this.Ex, Vector3.Cross(this.Ey, b));
        return x;
    }

    /// Solve A * x = b, where b is a column vector. This is more efficient
    /// than computing the inverse in one-shot cases. Solve only the upper
    /// 2-by-2 matrix equation.
    public Vector2 Solve22(in Vector2 b)
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

        Vector2 x;
        x.X = det * (a22 * b.X - a12 * b.Y);
        x.Y = det * (a11 * b.Y - a21 * b.X);
        return x;
    }

    /// Get the inverse of this matrix as a 2-by-2.
    /// Returns the zero matrix if singular.
    public void GetInverse22(ref Matrix3x3 matrix3x3)
    {
        float a = this.Ex.X, b = this.Ey.X, c = this.Ex.Y, d = this.Ey.Y;
        float det = a * d - b * c;
        if (!det.Equals(0.0f))
        {
            det = 1.0f / det;
        }

        matrix3x3.Ex.X = det * d;
        matrix3x3.Ey.X = -det * b;
        matrix3x3.Ex.Z = 0.0f;
        matrix3x3.Ex.Y = -det * c;
        matrix3x3.Ey.Y = det * a;
        matrix3x3.Ey.Z = 0.0f;
        matrix3x3.Ez.X = 0.0f;
        matrix3x3.Ez.Y = 0.0f;
        matrix3x3.Ez.Z = 0.0f;
    }

    /// Get the symmetric inverse of this matrix as a 3-by-3.
    /// Returns the zero matrix if singular.
    public void GetSymInverse33(ref Matrix3x3 matrix3x3)
    {
        float det = Vector3.Dot(this.Ex, Vector3.Cross(this.Ey, this.Ez));
        if (!det.Equals(0.0f))
        {
            det = 1.0f / det;
        }

        float a11 = this.Ex.X, a12 = this.Ey.X, a13 = this.Ez.X;
        float a22 = this.Ey.Y, a23 = this.Ez.Y;
        float a33 = this.Ez.Z;

        matrix3x3.Ex.X = det * (a22 * a33 - a23 * a23);
        matrix3x3.Ex.Y = det * (a13 * a23 - a12 * a33);
        matrix3x3.Ex.Z = det * (a12 * a23 - a13 * a22);

        matrix3x3.Ey.X = matrix3x3.Ex.Y;
        matrix3x3.Ey.Y = det * (a11 * a33 - a13 * a13);
        matrix3x3.Ey.Z = det * (a13 * a12 - a11 * a23);

        matrix3x3.Ez.X = matrix3x3.Ex.Z;
        matrix3x3.Ez.Y = matrix3x3.Ey.Z;
        matrix3x3.Ez.Z = det * (a11 * a22 - a12 * a12);
    }
}