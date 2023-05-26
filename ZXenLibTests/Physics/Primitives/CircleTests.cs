namespace ZXenLibTests.Physics.Primitives;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using ZxenLib.Physics.Primitives;

[TestClass]
public class CircleTests
{
    [DataTestMethod]
    [DataRow(0f, 0f, 1f)]
    [DataRow(3f, 4f, 5f)]
    public void Constructor_SetsValuesCorrectly(float x, float y, float radius)
    {
        Circle circle = new Circle(x, y, radius);

        Assert.AreEqual(x, circle.X);
        Assert.AreEqual(y, circle.Y);
        Assert.AreEqual(radius, circle.Radius);
    }

    [DataTestMethod]
    [DataRow(6, 6, true)]
    [DataRow(10, 5, true)]
    [DataRow(11, 11, false)]
    public void Contains_Point_ReturnsExpectedResult(int x, int y, bool expectedResult)
    {
        Circle circle = new Circle(5, 5, 5);
        Point point = new Point(x, y);

        Assert.AreEqual(expectedResult, circle.Contains(point));
    }

    [DataTestMethod]
    [DataRow(6, 6, true)]
    [DataRow(10, 5, true)]
    [DataRow(11, 11, false)]
    public void Contains_Vector2_ReturnsExpectedResult(float x, float y, bool expectedResult)
    {
        Circle circle = new Circle(5, 5, 5);
        Vector2 vector2 = new Vector2(x, y);

        Assert.AreEqual(expectedResult, circle.Contains(vector2));
    }

    [DataTestMethod]
    [DataRow(6, 6, true)]
    [DataRow(10, 5, true)]
    [DataRow(11, 11, false)]
    public void Contains_XY_ReturnsExpectedResult(double x, double y, bool expectedResult)
    {
        Circle circle = new Circle(5, 5, 5);

        Assert.AreEqual(expectedResult, circle.Contains(x, y));
    }
}