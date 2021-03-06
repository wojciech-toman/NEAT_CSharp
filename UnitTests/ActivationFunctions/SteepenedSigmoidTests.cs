﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using NEAT_CSharp.ActivationFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT_CSharp.ActivationFunctions.Tests
{
    [TestClass()]
    public class SteepenedSigmoidTests
    {
        [DataTestMethod()]
        [DataRow(-10.0f, 0.0f)]
        [DataRow(0.0f, 0.5f)]
        [DataRow(0.5f, 0.9214f)]
        [DataRow(3.0f, 1.0f)]
        public void CalculateActivationTest(float input, float expected)
        {
            float epsilon = 0.0001f;

            ActivationFunction activation = new SteepenedSigmoid();
            float result = activation.CalculateActivation(input);
            Assert.IsTrue(Math.Abs(result - expected) < epsilon, $"Expected value <{expected}>. Actual <{result}>");
        }
    }
}