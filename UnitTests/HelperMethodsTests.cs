using Microsoft.VisualStudio.TestTools.UnitTesting;
using NEAT_CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT_CSharp.Tests
{
    [TestClass()]
    public class HelperMethodsTests
    {
        [DataTestMethod()]
        [DataRow(1, 5, 1, 1)]
        [DataRow(1, 5, 5, 5)]
        [DataRow(1, 5, 0, 1)]
        [DataRow(1, 5, 6, 5)]
        [DataRow(1, 5, 2, 2)]
        public void ClampTest_TestInteger(int min, int max, int value, int expected)
        {
            int retVal = value.Clamp(min, max);
            Assert.AreEqual(expected, retVal);
        }

        [DataTestMethod()]
        [DataRow(1, 5, 1, 1)]
        [DataRow(1, 5, 5, 5)]
        [DataRow(1, 5, 0, 1)]
        [DataRow(1, 5, 6, 5)]
        [DataRow(1, 5, 2, 2)]
        public void ClampTest_TestFloat(float min, float max, float value, float expected)
        {
            float retVal = value.Clamp(min, max);
            Assert.AreEqual(expected, retVal);
        }
    }
}