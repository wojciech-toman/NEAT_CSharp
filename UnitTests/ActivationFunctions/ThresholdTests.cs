using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeuralEvolution.ActivationFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralEvolution.ActivationFunctions.Tests
{
    [TestClass()]
    public class ThresholdTests
    {
        [DataTestMethod()]
        [DataRow(-2.0f, 0.0f)]
        [DataRow(-1.0f, 0.0f)]
        [DataRow(0.0f, 1.0f)]
        [DataRow(1.0f, 1.0f)]
        [DataRow(2.0f, 1.0f)]
        public void CalculateActivationTest(float input, float expected)
        {
            float epsilon = 0.0001f;

            ActivationFunction activation = new Threshold();
            float result = activation.CalculateActivation(input);
            Assert.IsTrue(Math.Abs(result - expected) < epsilon, $"Expected value <{expected}>. Actual <{result}>");
        }
    }
}