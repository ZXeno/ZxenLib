namespace ZXenLibTests.Physics;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using ZxenLib.Physics;
using ZxenLib.Physics.Primitives;

[TestClass]
public class IntersectionDetector2DTests
{
    [DataTestMethod]
    [DataRow(0f, 0f, 0f, 0f, 1f, 1f, true)] // Point on line
    [DataRow(1f, 1f, 0f, 0f, 0f, 1f, false)] // Point not on line
    public void PointOnLine_ReturnsExpectedResult(float px, float py, float lx1, float ly1, float lx2, float ly2, bool expected)
    {
        Vector2 point = new Vector2(px, py);
        Line2D line = new Line2D(new Vector2(lx1, ly1), new Vector2(lx2, ly2));

        bool result = IntersectionDetector2D.PointOnLine(point, line);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void CircleContains_ReturnsTrue_WhenCircleContainsPoint()
    {
        Circle circle = new Circle(0, 0, 1);
        Vector2 point = new Vector2(0, 0);

        bool result = IntersectionDetector2D.CircleContains(point, circle);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void CircleContains_ReturnsFalse_WhenCircleDoesNotContainPoint()
    {
        Circle circle = new Circle(0, 0, 1);
        Vector2 point = new Vector2(2, 0);

        bool result = IntersectionDetector2D.CircleContains(point, circle);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void AabbContains_ReturnsTrue_WhenAABBContainsPoint()
    {
        AABB box = new AABB(new Vector2(-1, -1), new Vector2(1, 1));
        Vector2 point = new Vector2(0, 0);

        bool result = IntersectionDetector2D.AabbContains(point, box);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Box2dContains_ReturnsTrue_WhenBoxContainsPoint()
    {
        Box2D box = new Box2D(new Vector2(-1, -1), new Vector2(1, 1));
        Vector2 point = new Vector2(0, 0);

        bool result = IntersectionDetector2D.Box2dContains(point, box);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Box2dContains_ReturnsFalse_WhenBoxDoesNotContainPoint()
    {
        Box2D box = new Box2D(new Vector2(-1, -1), new Vector2(1, 1));
        Vector2 point = new Vector2(2, 0);

        bool result = IntersectionDetector2D.Box2dContains(point, box);

        Assert.IsFalse(result);
    }

    [DataTestMethod]
    [DataRow(0f, 0f, 1f, -1f, 0f, 1f, 0f)]
    [DataRow(3.5f, 17f, 5f, -22f, -9f, 10f, 22f)]
    [DataRow(0f, 0f, 1f, -1f, 0f, 1f, 0f)]
    [DataRow(0f, 0f, 1f, -1f, 0f, 1f, 0f)]
    public void LineIntersectsCircle_ReturnsTrue_WhenLineIntersectsCircle(
        float circleX,
        float circleY,
        float rad,
        float lineStartX,
        float lineStartY,
        float lineEndX,
        float lineEndY)
    {
        Circle circle = new Circle(circleX, circleY, rad);
        Line2D line = new Line2D(new Vector2(lineStartX, lineStartY), new Vector2(lineEndX, lineEndY));

        bool result = IntersectionDetector2D.LineIntersectsCircle(line, circle);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void LineIntersectsCircle_ReturnsTrue_WhenLineEndPointsAreInsideCircle()
    {
        Circle circle = new Circle(0, 0, 3);
        Line2D line = new Line2D(new Vector2(-1, 0), new Vector2(1, 0));

        bool result = IntersectionDetector2D.LineIntersectsCircle(line, circle);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void LineIntersectsCircle_ReturnsFalse_WhenLineDoesNotIntersectCircle()
    {
        Circle circle = new Circle(0, 0, 1);
        Line2D line = new Line2D(new Vector2(-2, -2), new Vector2(-2, 2));

        bool result = IntersectionDetector2D.LineIntersectsCircle(line, circle);

        Assert.IsFalse(result);
    }

    [DataTestMethod]
    [DataRow(-1, -1, 1, 1, -2, 0, 2, 0)]
    [DataRow(-1, -1, 1, 1, 0, -2, 0, 2)]
    [DataRow(-1, -1, 1, 1, -2, 0, 2, 1)]
    [DataRow(-1, -1, 1, 1, 0, -2, 1, 2)]
    [DataRow(-1, -1, 1, 1, -2, -2, 2, 2)]
    [DataRow(-1, -1, 1, 1, 2, 2, -2, -2)]
    public void LineIntersectsAabb_ReturnsTrue_WhenLineIntersectsAabb(
        float minX,
        float minY,
        float maxX,
        float maxY,
        float lstartX,
        float lstartY,
        float lendX,
        float lendY)
    {
        AABB box = new AABB(new Vector2(minX, minY), new Vector2(maxX, maxY));
        Line2D line = new Line2D(new Vector2(lstartX, lstartY), new Vector2(lendX, lendY));

        bool result = IntersectionDetector2D.LineIntersectsAabb(line, box);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void LineIntersectsAabb_ReturnsFalse_WhenLineDoesNotIntersectAabb()
    {
        AABB box = new AABB(new Vector2(-1, -1), new Vector2(1, 1));
        Line2D line = new Line2D(new Vector2(-2, -2), new Vector2(-2, 2));

        bool result = IntersectionDetector2D.LineIntersectsAabb(line, box);

        Assert.IsFalse(result);
    }

    [DataTestMethod]
    [DataRow(-1, -1, 1, 1, -2, 0, 2, 0)]
    [DataRow(-1, -1, 1, 1, 0, -2, 0, 2)]
    [DataRow(-1, -1, 1, 1, -2, 0, 2, 1)]
    [DataRow(-1, -1, 1, 1, 0, -2, 1, 2)]
    [DataRow(-1, -1, 1, 1, -2, -2, 2, 2)]
    [DataRow(-1, -1, 1, 1, 2, 2, -2, -2)]
    [DataRow(0, 0, 8, 8, 3, -2, 11, 4)]
    public void LineIntersectsBox2D_ReturnsTrue_WhenLineIntersectsBox(
        float minX,
        float minY,
        float maxX,
        float maxY,
        float lstartX,
        float lstartY,
        float lendX,
        float lendY)
    {
        Box2D box = new Box2D(new Vector2(minX, minY), new Vector2(maxX, maxY));
        Line2D line = new Line2D(new Vector2(lstartX, lstartY), new Vector2(lendX, lendY));

        bool result = IntersectionDetector2D.LineIntersectsBox2D(line, box);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void LineIntersectsBox2D_ReturnsFalse_WhenLineDoesNotIntersectBox()
    {
        Box2D box = new Box2D(new Vector2(-1, -1), new Vector2(1, 1));
        Line2D line = new Line2D(new Vector2(-2, -2), new Vector2(-2, 2));

        bool result = IntersectionDetector2D.LineIntersectsBox2D(line, box);

        Assert.IsFalse(result);
    }
}
