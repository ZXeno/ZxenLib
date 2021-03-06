﻿namespace ZXenLibTests
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZxenLib.Configuration;

    [TestClass]
    public class ConfigSystemTests
    {
        [TestMethod]
        public async Task ConfigSystemSavesAndLoadsCustomPropertiesCorrectly()
        {
            ConfigurationManager cm = new ConfigurationManager();
            cm.GameSettingsDirectory = Path.Combine(Environment.CurrentDirectory, "CONFIG_TEST");

            cm.Config.SetConfigProperty("test", "test");
            cm.Config.SetConfigProperty("intTest", 1);
            cm.Config.SetConfigProperty("sbyteTest", (byte)1);
            cm.Config.SetConfigProperty("short", (short)1);
            cm.Config.SetConfigProperty("ushort", (ushort)1);
            cm.Config.SetConfigProperty("float", (float)1);
            cm.Config.SetConfigProperty("long", (long)1);
            cm.Config.SetConfigProperty("ulong", (ulong)1);
            cm.Config.SetConfigProperty("double", (double)1);

            var test = cm.GetConfigOption<string>("test");
            var intTest = cm.GetConfigOption<int>("intTest");
            var sbyteTest = cm.GetConfigOption<byte>("sbyteTest");

            Assert.AreEqual(test, "test");
            Assert.AreEqual(intTest, 1);
            Assert.AreEqual(sbyteTest, (byte)1);

            await cm.SaveConfiguration();
            cm.LoadConfiguration();

            test = cm.GetConfigOption<string>("test");
            intTest = cm.GetConfigOption<int>("intTest");
            sbyteTest = cm.GetConfigOption<byte>("sbyteTest");
            var xtest = cm.GetConfigOption<double>("double");

            Assert.AreEqual(test, "test");
            Assert.AreEqual(intTest, 1);
            Assert.AreEqual(sbyteTest, (byte)1);
        }
    }
}
