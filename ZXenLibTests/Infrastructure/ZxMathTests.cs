namespace ZXenLibTests.Infrastructure;

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using ZxenLib;

[TestClass]
public class ZxMathTests
{
    private const float Epsilon = 0.00001f; // for float comparison

    [DataTestMethod]
    [DataRow(5.0f, 5.0f, 0.0f, true)]
    [DataRow(1.0f, 1.000001f, Epsilon, true)]
    [DataRow(1.0f, 2.0f, Epsilon, false)]
    public void Compare_True_SatisfiesEpsilonCondition(float x, float y, float epsilon, bool expected)
    {
        bool result = ZxMath.Compare(x, y, epsilon);

        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [DataRow(1.0f, 1.0f, 1.0f, 1.0f, Epsilon, true)]
    [DataRow(1.0f, 1.0001f, 1.0f, 1.0001f, Epsilon, true)]
    [DataRow(1.0f, 2.0f, 1.0f, 2.0f, Epsilon, true)]
    public void CompareVector_True_SatisfiesEpsilonCondition(float x1, float y1, float x2, float y2, float epsilon, bool expected)
    {
        Vector2 v1 = new Vector2(x1, y1);
        Vector2 v2 = new Vector2(x2, y2);

        bool result = ZxMath.Compare(v1, v2, epsilon);

        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [DataRow(0.0f, 0.0f, 0.0f, 0.0f, 90.0f, true, 0.0f, 0.0f)]
    [DataRow(1.0f, 1.0f, 0.0f, 0.0f, 90.0f, true, -1.0f, 1.0f)]
    [DataRow(0.0f, 0.0f, 0.0f, 0.0f, 90.0f, true, 0.0f, 0.0f)]
    [DataRow(1.2345f, 0.5432f, 0.1111f, 0.2222f, 45.0f, true, 0.678482533f, 1.24354517f)]
    [DataRow(0.9876f, 0.1234f, 0.3333f, 0.4444f, 180.0f, true, -.32100004f, 0.765399933f)]
    [DataRow(1.5432f, 2.3456f, 0.5555f, 0.6666f, 270.0f, true, 2.23449993f, -0.321100056f)]
    [DataRow(3.4567f, 4.6789f, 0.7777f, 0.8888f, 360.0f, true, 3.45669937f, 4.67890024f)]
    [DataRow(5.6789f, 6.7890f, 0.9999f, 1.0000f, 90.0f, false, -6.27198315f, 2.58911228f)]
    public void RotateCoords_Result_Condition(
        float pX,
        float pY,
        float oX,
        float oY,
        double rotation,
        bool useDegrees,
        float expectedX,
        float expectedY)
    {
        Span<float> expected = new Span<float>(new float[] { expectedX, expectedY });
        Span<float> result = ZxMath.RotateCoords(pX, pY, oX, oY, rotation, useDegrees);

        bool isEqual = ZxMath.Compare(result[0], expected[0], Epsilon) &&
                       ZxMath.Compare(result[1], expected[1], Epsilon);

        Assert.IsTrue(isEqual);
    }
}