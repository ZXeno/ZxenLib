namespace ZXenLibTests.Extensions;

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZxenLib.Extensions;

[TestClass]
public class RandomExtensionsTests
{
    private Random _random;

    [TestInitialize]
    public void TestInit()
    {
        this._random = new Random(50);
    }

    [TestMethod]
    public void Test_RangeSingle_With_Min_And_Max_Being_Equal()
    {
        float min = 10f;
        float max = 10f;

        float result = this._random.RangeSingle(min, max);

        Assert.AreEqual(min, result);
    }

    [TestMethod]
    public void Test_RangeSingle_With_Positive_Numbers()
    {
        float min = 1f;
        float max = 100f;

        float result = this._random.RangeSingle(min, max);

        Assert.IsTrue(result >= min && result <= max);
    }

    [TestMethod]
    public void Test_RangeSingle_With_Negative_Numbers()
    {
        float min = -100f;
        float max = -1f;

        float result = this._random.RangeSingle(min, max);

        Assert.IsTrue(result >= min && result <= max);
    }

    [TestMethod]
    public void Test_RangeSingle_With_Zero_Boundaries()
    {
        float min = -100f;
        float max = 100f;

        float result = this._random.RangeSingle(min, max);

        Assert.IsTrue(result >= min && result <= max);
    }

    [TestMethod]
    public void Test_RangeSingle_With_Large_Numbers()
    {
        float min = float.MinValue;
        float max = float.MaxValue;

        float result = this._random.RangeSingle(min, max);

        Assert.IsTrue(result >= min && result <= max);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Test_RangeSingle_With_Min_Greater_Than_Max()
    {
        float min = 100f;
        float max = 1f;

        float result = this._random.RangeSingle(min, max);
    }
}
