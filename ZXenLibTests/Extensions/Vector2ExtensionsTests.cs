namespace ZXenLibTests.Extensions;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using ZxenLib.Extensions;

[TestClass]
public class Vector2ExtensionsTests
{
    // Note: in practice you might want to use a more appropriate precision level for your needs
    private const float Precision = 0.00001f;

    [DataTestMethod]
    [DataRow(1f, 1f, 1f, 1f, 90f, true, 1f, 1f)]
    [DataRow(2f, 2f, 1f, 1f, 90f, true, 0f, 2f)]
    [DataRow(2f, 2f, 1f, 1f, 45f, true, 0.99999994f, 2.4142137f)]
    [DataRow(2f, 2f, 1f, 1f, 0.7854f, false, 0.99999994f, 2.4142137f)]
    public void Rotate_ChangesVectorCoordinatesAccordingToRotation(
        float initialX, float initialY,
        float originX, float originY,
        double rotation, bool useDegrees,
        float expectedX, float expectedY)
    {
        // Arrange
        Vector2 vector = new Vector2(initialX, initialY);
        Vector2 origin = new Vector2(originX, originY);

        // Act
        vector = vector.Rotate(origin, rotation, useDegrees);

        // Assert
        Assert.AreEqual(expectedX, vector.X, Precision);
        Assert.AreEqual(expectedY, vector.Y, Precision);
    }
}