namespace ZXenLibTests.Extensions;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZxenLib.Extensions;

[TestClass]
public class FloatExtensionsTests
{
    [DataTestMethod]
    [DataRow(0f, 0f)]
    [DataRow(1f, 1f)]
    [DataRow(-1f, 1f)]
    [DataRow(float.MinValue, float.MaxValue)]
    public void WithinThreshold_X_IsWithinThreshold(float x, float epsilon)
    {
        bool result = x.WithinThreshold(epsilon);

        Assert.IsTrue(result);
    }

    [DataTestMethod]
    [DataRow(2f, 1f, 1f)]
    [DataRow(0.5f, 0f, 1f)]
    [DataRow(-1f, -2f, 1f)]
    public void Compare_XY_IsWithinThreshold(float x, float y, float epsilon)
    {
        bool result = x.Compare(y, epsilon);

        Assert.IsTrue(result);
    }

    [DataTestMethod]
    [DataRow(1f, .2f)]
    [DataRow(-1f, .2f)]
    [DataRow(124.1234f, 124.12333f)]
    public void WithinThreshold_X_IsNotWithinThreshold(float x, float epsilon)
    {
        bool result = x.WithinThreshold(epsilon);

        Assert.IsFalse(result);
    }

    [DataTestMethod]
    [DataRow(2f, 1f, 0.49999f)]
    [DataRow(2f, 1f, 0.5f)]
    [DataRow(0.5f, 0f, 0.25f)]
    [DataRow(-1f, -2f, 0.5f)]
    public void Compare_XY_IsNotWithinThreshold(float x, float y, float epsilon)
    {
        bool result = x.Compare(y, epsilon);

        Assert.IsFalse(result);
    }

    [DataTestMethod]
    [DataRow(1f, 1f, 0f)]
    [DataRow(0f, 0f, 0f)]
    [DataRow(-1f, -1f, 0f)]
    public void Compare_XY_AreEqual_EpsilonIsZero(float x, float y, float epsilon)
    {
        bool result = x.Compare(y, epsilon);

        Assert.IsTrue(result);
    }

    [DataTestMethod]
    [DataRow(1000000f, 0.000001f, 0.1f)]
    [DataRow(-1000000f, -0.000001f, 0.1f)]
    public void Compare_XY_DifferSignificantly_EpsilonIsSmall(float x, float y, float epsilon)
    {
        bool result = x.Compare(y, epsilon);

        Assert.IsFalse(result);
    }
}