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
    public void ShapeContains_ReturnsTrue_WhenCircleContainsPoint()
    {
        Circle circle = new Circle(0, 0, 1);
        Vector2 point = new Vector2(0, 0);

        bool result = IntersectionDetector2D.ShapeContains(point, circle);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShapeContains_ReturnsFalse_WhenCircleDoesNotContainPoint()
    {
        Circle circle = new Circle(0, 0, 1);
        Vector2 point = new Vector2(2, 0);

        bool result = IntersectionDetector2D.ShapeContains(point, circle);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ShapeContains_ReturnsTrue_WhenAABBContainsPoint()
    {
        AABB box = new AABB(new Vector2(-1, -1), new Vector2(1, 1));
        Vector2 point = new Vector2(0, 0);

        bool result = IntersectionDetector2D.ShapeContains(point, box);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShapeContains_ReturnsTrue_WhenBoxContainsPoint()
    {
        Box2D box = new Box2D(new Vector2(-1, -1), new Vector2(1, 1));
        Vector2 point = new Vector2(0, 0);

        bool result = IntersectionDetector2D.ShapeContains(point, box);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShapeContains_ReturnsFalse_WhenBoxDoesNotContainPoint()
    {
        Box2D box = new Box2D(new Vector2(-1, -1), new Vector2(1, 1));
        Vector2 point = new Vector2(2, 0);

        bool result = IntersectionDetector2D.ShapeContains(point, box);

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
    public void LineIntersectsPolygon_ReturnsTrue_WhenLineIntersectsAabb(
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

        bool result = IntersectionDetector2D.LineIntersectsPolygon(line, box);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void LineIntersectsPolygon_ReturnsFalse_WhenLineDoesNotIntersectAabb()
    {
        AABB box = new AABB(new Vector2(-1, -1), new Vector2(1, 1));
        Line2D line = new Line2D(new Vector2(-2, -2), new Vector2(-2, 2));

        bool result = IntersectionDetector2D.LineIntersectsPolygon(line, box);

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
    public void LineIntersectsPolygon_ReturnsTrue_WhenLineIntersectsBox(
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

        bool result = IntersectionDetector2D.LineIntersectsPolygon(line, box);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void LineIntersectsPolygon_ReturnsFalse_WhenLineDoesNotIntersectBox()
    {
        Box2D box = new Box2D(new Vector2(-1, -1), new Vector2(1, 1));
        Line2D line = new Line2D(new Vector2(-2, -2), new Vector2(-2, 2));

        bool result = IntersectionDetector2D.LineIntersectsPolygon(line, box);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Raycast_ShouldReturnTrue_WhenRayIntersectsCircle()
    {
        // Arrange
        Circle circle = new Circle(1, 1, 1);
        Ray2D ray = new Ray2D(new Vector2(0, 0), new Vector2(1, 1));
        RaycastResult result = new RaycastResult();

        // Act
        bool doesIntersect = IntersectionDetector2D.Raycast(circle, ray, result);

        // Assert
        Assert.IsTrue(doesIntersect);
        Assert.IsTrue(result.Hit);
    }

    [TestMethod]
    public void Raycast_ShouldReturnFalse_WhenRayDoesNotIntersectCircle()
    {
        // Arrange
        Circle circle = new Circle(10, 10, 1);
        Ray2D ray = new Ray2D(new Vector2(0, 0), new Vector2(1, 0));
        RaycastResult result = new RaycastResult();

        // Act
        bool doesIntersect = IntersectionDetector2D.Raycast(circle, ray, result);

        // Assert
        Assert.IsFalse(doesIntersect);
        Assert.IsFalse(result.Hit);
    }

    [TestMethod]
    public void Raycast_ShouldReturnTrue_WhenRayOriginIsInsideCircle()
    {
        // Arrange
        Circle circle = new Circle(1, 1, 5);
        Ray2D ray = new Ray2D(new Vector2(1, 1), new Vector2(1, 0));
        RaycastResult result = new RaycastResult();

        // Act
        bool doesIntersect = IntersectionDetector2D.Raycast(circle, ray, result);

        // Assert
        Assert.IsTrue(doesIntersect);
        Assert.IsTrue(result.Hit);
    }

    [TestMethod]
    public void Raycast_ShouldNotModifyRaycastResult_WhenRaycastResultIsNull()
    {
        // Arrange
        Circle circle = new Circle(1, 1, 5);
        Ray2D ray = new Ray2D(new Vector2(1, 1), new Vector2(1, 0));
        RaycastResult raycastResult = null;

        // Act
        bool doesIntersect = IntersectionDetector2D.Raycast(circle, ray, raycastResult);

        // Assert
        Assert.IsTrue(doesIntersect);
        Assert.IsNull(raycastResult);
    }

    [TestMethod]
    public void Raycast_AABB_ShouldReturnTrue_WhenRayIntersectsBox()
    {
        // Arrange
        AABB box = new AABB(new Vector2(0, 0), new Vector2(2, 2));
        Ray2D ray = new Ray2D(new Vector2(-1, 1), new Vector2(2, 1));
        RaycastResult result = new RaycastResult();

        // Act
        bool doesIntersect = IntersectionDetector2D.Raycast(box, ray, result);

        // Assert
        Assert.IsTrue(doesIntersect);
        Assert.IsTrue(result.Hit);
    }

    [TestMethod]
    public void Raycast_AABB_ShouldReturnFalse_WhenRayDoesNotIntersectBox()
    {
        // Arrange
        AABB box = new AABB(new Vector2(2, 2), new Vector2(3, 3));
        Ray2D ray = new Ray2D(new Vector2(0, 0), new Vector2(1, 0));
        RaycastResult result = new RaycastResult();

        // Act
        bool doesIntersect = IntersectionDetector2D.Raycast(box, ray, result);

        // Assert
        Assert.IsFalse(doesIntersect);
        Assert.IsFalse(result.Hit);
    }

    [TestMethod]
    public void Raycast_Box2D_ShouldReturnTrue_WhenRayIntersectsBox()
    {
        // Arrange
        Box2D box = new Box2D(new Vector2(2f), new Vector2(), 0f);
        Ray2D ray = new Ray2D(new Vector2(-2, 0), new Vector2(2, 0));
        RaycastResult result = new RaycastResult();

        // Act
        bool doesIntersect = IntersectionDetector2D.Raycast(box, ray, result);

        // Assert
        Assert.IsTrue(doesIntersect);
        Assert.IsTrue(result.Hit);
    }

    [TestMethod]
    public void Raycast_Box2D_ShouldReturnFalse_WhenRayDoesNotIntersectBox()
    {
        // Arrange
        Box2D box = new Box2D(new Vector2(2f), new Vector2(2f), 0f);
        Ray2D ray = new Ray2D(new Vector2(0, 0), new Vector2(1, 0));
        RaycastResult result = new RaycastResult();

        // Act
        bool doesIntersect = IntersectionDetector2D.Raycast(box, ray, result);

        // Assert
        Assert.IsFalse(doesIntersect);
        Assert.IsFalse(result.Hit);
    }

    [TestMethod]
    public void Raycast_Box2D_ShouldReturnTrue_WhenRayIntersectsRotatedBox()
    {
        // Arrange
        Box2D box = new Box2D(new Vector2(2), new Vector2(0), 45);
        Ray2D ray = new Ray2D(new Vector2(-2.5f, 1), new Vector2(1, -1));
        RaycastResult result = new RaycastResult();

        // Act
        bool doesIntersect = IntersectionDetector2D.Raycast(box, ray, result);

        // Assert
        Assert.IsTrue(doesIntersect);
        Assert.IsTrue(result.Hit);
    }

    [TestMethod]
    public void CircleVsCircle_ShouldReturnTrue_WhenCirclesOverlap()
    {
        // Arrange
        Circle circle1 = new Circle(new Vector2(0, 0), 1);
        Circle circle2 = new Circle(new Vector2(1, 0), 1);

        // Act
        bool doOverlap = IntersectionDetector2D.CircleVsCircle(circle1, circle2);

        // Assert
        Assert.IsTrue(doOverlap);
    }

    [TestMethod]
    public void CircleVsCircle_ShouldReturnFalse_WhenCirclesDoNotOverlap()
    {
        // Arrange
        Circle circle1 = new Circle(new Vector2(0, 0), 1);
        Circle circle2 = new Circle(new Vector2(3, 0), 1);

        // Act
        bool doOverlap = IntersectionDetector2D.CircleVsCircle(circle1, circle2);

        // Assert
        Assert.IsFalse(doOverlap);
    }

    [TestMethod]
    public void CircleVsPolygon_ShouldReturnTrue_WhenCircleIntersectsAABB()
    {
        // Arrange
        Circle circle = new Circle(new Vector2(0, 0), 1);
        AABB box = new AABB(new Vector2(-1, -1), new Vector2(1, 1));

        // Act
        bool doesIntersect = IntersectionDetector2D.CircleVsPolygon(circle, box);

        // Assert
        Assert.IsTrue(doesIntersect);
    }

    [TestMethod]
    public void CircleVsPolygon_ShouldReturnFalse_WhenCircleDoesNotIntersectAABB()
    {
        // Arrange
        Circle circle = new Circle(new Vector2(3, 0), 1);
        AABB box = new AABB(new Vector2(-1, -1), new Vector2(1, 1));

        // Act
        bool doesIntersect = IntersectionDetector2D.CircleVsPolygon(circle, box);

        // Assert
        Assert.IsFalse(doesIntersect);
    }

    [TestMethod]
    public void CircleVsPolygon_ShouldReturnTrue_WhenCircleIntersectsBox2D()
    {
        // Arrange
        Circle circle = new Circle(new Vector2(0, 0), 2.5f);
        Box2D box = new Box2D(new Vector2(1), new Vector2(2, 2), 0);

        // Act
        bool doesIntersect = IntersectionDetector2D.CircleVsPolygon(circle, box);

        // Assert
        Assert.IsTrue(doesIntersect);
    }

    [TestMethod]
    public void CircleVsPolygon_ShouldReturnFalse_WhenCircleDoesNotIntersectBox2D()
    {
        // Arrange
        Circle circle = new Circle(new Vector2(3, 0), 1f);
        Box2D box = new Box2D(new Vector2(1, 1), new Vector2(2, 2), 0);

        // Act
        bool doesIntersect = IntersectionDetector2D.CircleVsPolygon(circle, box);

        // Assert
        Assert.IsFalse(doesIntersect);
    }

    [TestMethod]
    public void VertexShapeVsVertexShape_ShouldReturnTrue_WhenShapeIntersects()
    {
        // Arrange
        AABB box1 = new AABB(new Vector2(0, 0), 2.5f);
        Box2D box2 = new Box2D(new Vector2(1,1), new Vector2(1.5f, 1.5f), 45);

        // Act
        bool doesIntersect = IntersectionDetector2D.VertexShapeVsVertexShape(box1, box2);

        // Assert
        Assert.IsTrue(doesIntersect);
    }

    [TestMethod]
    public void VertexShapeVsVertexShape_ShouldReturnFalse_WhenShapeDoesNotIntersect()
    {
        // Arrange
        AABB box1 = new AABB(new Vector2(3, 0), 1f);
        Box2D box2 = new Box2D(new Vector2(1, 1), new Vector2(2, 2), 0);

        // Act
        bool doesIntersect = IntersectionDetector2D.VertexShapeVsVertexShape(box1, box2);

        // Assert
        Assert.IsFalse(doesIntersect);
    }
}
