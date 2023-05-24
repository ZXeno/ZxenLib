namespace ZXenLibTests.Physics.Primitives;

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using ZxenLib.Physics.Primitives;

[TestClass]
public class Box2DTests
{
    [DataTestMethod]
    [DataRow(0f, 0f, 0f, 0f, 1f, 1f)] // Point is at the center
    [DataRow(0f, 0f, 0.5f, 0f, 1f, 1f)] // Point is within the box
    [DataRow(0f, 0f, .5f, 0f, 1f, 1f)] // Point is on the boundary
    public void Contains_ReturnsTrue(float boxX, float boxY, float pointX, float pointY, float boxSizeX, float boxSizeY)
    {
        // Arrange
        Box2D box = new()
        {
            Position = new(boxX, boxY),
            Size = new(boxSizeX, boxSizeY),
        };

        // Act
        bool result = box.Contains(new Vector2(pointX, pointY));

        // Assert
        Assert.IsTrue(result);
    }

    [DataTestMethod]
    [DataRow(0f, 0f, 2f, 0f, 1f, 1f)] // Point is outside the box
    public void Contains_ReturnsFalse(float boxX, float boxY, float pointX, float pointY, float boxSizeX, float boxSizeY)
    {
        Box2D box = new()
        {
            Position = new(boxX, boxY),
            Size = new(boxSizeX, boxSizeY),
        };

        bool result = box.Contains(new Vector2(pointX, pointY));

        Assert.IsFalse(result);
    }

    [DataTestMethod]
    [DataRow(0f, 0f, 1f, 1f)] // Box is at the origin
    [DataRow(1f, 1f, 1f, 1f)] // Box is at position (1, 1)
    public void GetMin_ReturnsExpectedValue(float boxX, float boxY, float sizeX, float sizeY)
    {
        Box2D box = new()
        {
            Position = new(boxX, boxY),
            Size = new(sizeX, sizeY),
        };

        Vector2 result = box.GetMin();

        Assert.AreEqual(new(boxX - sizeX / 2f, boxY - sizeY / 2f), result);
    }

    [DataTestMethod]
    [DataRow(0f, 0f, 1f, 1f)] // Box is at the origin
    [DataRow(1f, 1f, 1f, 1f)] // Box is at position (1, 1)
    public void GetMax_ReturnsExpectedValue(float boxX, float boxY, float sizeX, float sizeY)
    {
        Box2D box = new Box2D
        {
            Position = new Vector2(boxX, boxY),
            Size = new Vector2(sizeX, sizeY),
        };

        Vector2 result = box.GetMax();

        Assert.AreEqual(new Vector2(boxX + sizeX / 2f, boxY + sizeY / 2f), result);
    }

    [TestMethod]
    public void DefaultConstructor_InitializesFieldsCorrectly()
    {
        Box2D box = new Box2D();

        Assert.AreEqual(new Vector2(0f), box.Position);
        Assert.AreEqual(new Vector2(1f), box.Size);
        Assert.AreEqual(0f, box.Rotation);
    }

    [TestMethod]
    public void MinMaxConstructor_InitializesFieldsCorrectly()
    {
        Vector2 min = new Vector2(0f, 0f);
        Vector2 max = new Vector2(1f, 1f);
        Box2D box = new Box2D(min, max);

        Assert.AreEqual(new Vector2(0.5f), box.Position);
        Assert.AreEqual(new Vector2(1f), box.Size);
        Assert.AreEqual(0f, box.Rotation);
    }

    [TestMethod]
    public void GetVerticies_ReturnsCorrectlyRotatedVertices()
    {
        Box2D box = new Box2D(new Vector2(-1f, -1f), new Vector2(1f, 1f));
        box.Rotation = MathHelper.ToDegrees((float)Math.PI / 2f); // 90 degrees

        Span<Vector2> vertices = box.GetVertices();

        // Assuming Rotate method rotates counter-clockwise
        Assert.IsTrue(box.Contains(vertices[0])); // should be top-left
        Assert.IsTrue(box.Contains(vertices[1])); // should be bottom-left
        Assert.IsTrue(box.Contains(vertices[2])); // should be top-right
        Assert.IsTrue(box.Contains(vertices[3])); // should be bottom-right
    }
}
