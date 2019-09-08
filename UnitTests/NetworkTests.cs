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
    public class NetworkTests
    {
        [TestMethod]
        public void activateTest_OneOutputNotConnected_Expected_False()
        {
            // Create network and leave one of the output nodes unconnected
            Network net = new Network();

            Node inNode1 = new Node(Node.ENodeType.SENSOR, 1);
            Node inNode2 = new Node(Node.ENodeType.SENSOR, 2);
            Node hiddenNode = new Node(Node.ENodeType.HIDDEN, 3);
            Node outNode1 = new Node(Node.ENodeType.OUTPUT, 4);
            Node outNode2 = new Node(Node.ENodeType.OUTPUT, 5);

            net.addNodes(new Node[] { inNode1, inNode2, hiddenNode, outNode1, outNode2 });

            net.addLink(new Link(inNode1, hiddenNode, false, 1.0f));
            net.addLink(new Link(inNode2, hiddenNode, false, 1.0f));
            net.addLink(new Link(hiddenNode, outNode1, false, 1.0f));

            Assert.AreEqual(false, net.activate());
        }

        [TestMethod]
        public void activateTest_AllOutputsConnected_Expected_True()
        {
            // Create network and make all outputs connected to the network
            Network net = new Network();

            Node inNode1 = new Node(Node.ENodeType.SENSOR, 1);
            Node inNode2 = new Node(Node.ENodeType.SENSOR, 2);
            Node hiddenNode = new Node(Node.ENodeType.HIDDEN, 3);
            Node outNode1 = new Node(Node.ENodeType.OUTPUT, 4);
            Node outNode2 = new Node(Node.ENodeType.OUTPUT, 5);

            net.addNodes(new Node[] { inNode1, inNode2, hiddenNode, outNode1, outNode2 });

            net.addLink(new Link(inNode1, hiddenNode, false, 1.0f));
            net.addLink(new Link(inNode2, hiddenNode, false, 1.0f));
            net.addLink(new Link(hiddenNode, outNode1, false, 1.0f));
            net.addLink(new Link(hiddenNode, outNode2, false, 1.0f));

            Assert.AreEqual(true, net.activate());
        }

        [TestMethod]
        public void activateTest_OneInputNotConnected_Expected_True()
        {
            // Create network and leave one of the input nodes unconnected
            Network net = new Network();

            Node inNode1 = new Node(Node.ENodeType.SENSOR, 1);
            Node inNode2 = new Node(Node.ENodeType.SENSOR, 2);
            Node hiddenNode = new Node(Node.ENodeType.HIDDEN, 3);
            Node outNode1 = new Node(Node.ENodeType.OUTPUT, 4);
            Node outNode2 = new Node(Node.ENodeType.OUTPUT, 5);

            net.addNodes(new Node[] { inNode1, inNode2, hiddenNode, outNode1, outNode2 });

            net.addLink(new Link(inNode2, hiddenNode, false, 1.0f));
            net.addLink(new Link(hiddenNode, outNode1, false, 1.0f));
            net.addLink(new Link(hiddenNode, outNode2, false, 1.0f));

            Assert.AreEqual(true, net.activate());
        }

        [TestMethod]
        public void activateTest_HiddenNotConnected_Expected_True()
        {
            // Create network and leave hidden layer not connected
            Network net = new Network();

            Node inNode1 = new Node(Node.ENodeType.SENSOR, 1);
            Node inNode2 = new Node(Node.ENodeType.SENSOR, 2);
            Node hiddenNode = new Node(Node.ENodeType.HIDDEN, 3);
            Node outNode1 = new Node(Node.ENodeType.OUTPUT, 4);
            Node outNode2 = new Node(Node.ENodeType.OUTPUT, 5);

            net.addNodes(new Node[] { inNode1, inNode2, hiddenNode, outNode1, outNode2 });

            net.addLink(new Link(inNode1, outNode1, false, 1.0f));
            net.addLink(new Link(inNode2, outNode2, false, 1.0f));

            Assert.AreEqual(true, net.activate());
        }

        [TestMethod]
        public void addNodes()
        {
            Network net = new Network();

            Node inNode1 = new Node(Node.ENodeType.SENSOR, 1);
            Node inNode2 = new Node(Node.ENodeType.SENSOR, 2);
            Node hiddenNode = new Node(Node.ENodeType.HIDDEN, 3);
            Node outNode1 = new Node(Node.ENodeType.OUTPUT, 4);
            Node outNode2 = new Node(Node.ENodeType.OUTPUT, 5);

            net.addNodes(new Node[] { inNode1, inNode2, hiddenNode, outNode1, outNode2 });

            Assert.AreEqual(5, net.Nodes.Count);
            Assert.AreEqual(2, net.Input.Count);
            Assert.AreEqual(2, net.Output.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void addNodes_Null_Expected_Exception()
        {
            Network net = new Network();
            net.addNodes(null);
        }

        [TestMethod()]
        public void getNodeByIdTest_NonExisting_Expected_Null()
        {
            Network net = new Network();

            Node inNode1 = new Node(Node.ENodeType.SENSOR, 1);
            Node inNode2 = new Node(Node.ENodeType.SENSOR, 2);
            Node hiddenNode = new Node(Node.ENodeType.HIDDEN, 3);
            Node outNode1 = new Node(Node.ENodeType.OUTPUT, 4);
            Node outNode2 = new Node(Node.ENodeType.OUTPUT, 5);

            net.addNodes(new Node[] { inNode1, inNode2, hiddenNode, outNode1, outNode2 });

            Assert.AreEqual(null, net.getNodeById(0));
            Assert.AreEqual(null, net.getNodeById(6));
        }

        [TestMethod()]
        public void getNodeByIdTest_Existing()
        {
            Network net = new Network();

            Node inNode1 = new Node(Node.ENodeType.SENSOR, 1);
            Node inNode2 = new Node(Node.ENodeType.SENSOR, 2);
            Node hiddenNode = new Node(Node.ENodeType.HIDDEN, 3);
            Node outNode1 = new Node(Node.ENodeType.OUTPUT, 4);
            Node outNode2 = new Node(Node.ENodeType.OUTPUT, 5);

            net.addNodes(new Node[] { inNode1, inNode2, hiddenNode, outNode1, outNode2 });

            Assert.AreEqual(inNode1, net.getNodeById(1));
            Assert.AreEqual(inNode2, net.getNodeById(2));
            Assert.AreEqual(hiddenNode, net.getNodeById(3));
            Assert.AreEqual(outNode1, net.getNodeById(4));
            Assert.AreEqual(outNode2, net.getNodeById(5));
        }

        [DataTestMethod()]
        [DataRow(3.0f, 1.0f, 1.0f, 1.0f)]
        [DataRow(3.0f, 1.0f, 1.0f, 0.0f)]
        [DataRow(2.5f, 0.5f, 1.0f, 1.0f)]
        [DataRow(2.0f, 1.0f, 0.5f, 1.0f)]
        public void ComputeNodesActivationSumTest(float expectedOutput, float weight1, float weight2, float weight3)
        {
            Network net = new Network();

            Node inNode1 = new Node(Node.ENodeType.SENSOR, 1);
            Node inNode2 = new Node(Node.ENodeType.SENSOR, 2);
            Node inNode3 = new Node(Node.ENodeType.OUTPUT, 3);
            Node hiddenNode = new Node(Node.ENodeType.HIDDEN, 4);
            Node outNode1 = new Node(Node.ENodeType.OUTPUT, 5);

            net.addNodes(new Node[] { inNode1, inNode2, inNode3, hiddenNode, outNode1 });

            inNode1.Activation = 1.0f; inNode1.ActivationCount = 1;
            inNode2.Activation = 2.0f; inNode2.ActivationCount = 1;
            inNode3.Activation = 4.0f; inNode3.ActivationCount = 1;

            net.addLink(new Link(inNode1, hiddenNode, false, weight1));
            net.addLink(new Link(inNode2, hiddenNode, false, weight2));
            net.addLink(new Link(hiddenNode, outNode1, false, weight3));

            net.ComputeNodesActivationSum();

            Assert.AreEqual(expectedOutput, hiddenNode.ActivationSum);
        }

        [DataTestMethod()]
        [DataRow(-10.0f, 0.0f)]
        [DataRow(0.0f, 0.5f)]
        [DataRow(0.5f, 0.9214f)]
        [DataRow(3.0f, 1.0f)]
        public void ComputeNodesActivationFunctionValueTest_DefaultFunction(float activationSum, float expected)
        {
            float epsilon = 0.0001f;

            Network net = new Network();
            Node hiddenNode = new Node(Node.ENodeType.HIDDEN, 1);
            net.addNode(hiddenNode);

            hiddenNode.ActivationSum = activationSum; hiddenNode.IsActive = true;

            net.ComputeNodesActivationFunctionValue();

            Assert.IsTrue(Math.Abs(hiddenNode.Activation - expected) < epsilon, $"Expected value <{expected}>. Actual <{hiddenNode.Activation}>");
        }

        [TestMethod()]
        [ExpectedException(typeof(NullReferenceException))]
        public void ComputeNodesActivationFunctionValueTest_NoActivationFunction_Expected_Exception()
        {
            Network net = new Network();
            Node hiddenNode = new Node(Node.ENodeType.HIDDEN, 1);
            net.addNode(hiddenNode);

            net.ActivationFunction = null;

            hiddenNode.ActivationSum = 3.0f; hiddenNode.IsActive = true;

            net.ComputeNodesActivationFunctionValue();
        }

        [Serializable]
        class TestActivationFunction : ActivationFunctions.ActivationFunction
        {
            public override float CalculateActivation(float input)
            {
                throw new NotImplementedException();
            }
        };

        [TestMethod()]
        public void SerializeDeserializeBinaryTest()
        {
        Network net = new Network();

            Node inNode1 = new Node(Node.ENodeType.SENSOR, 1);
            Node inNode2 = new Node(Node.ENodeType.SENSOR, 2);
            Node hiddenNode = new Node(Node.ENodeType.HIDDEN, 3);
            Node outNode1 = new Node(Node.ENodeType.OUTPUT, 4);
            Node outNode2 = new Node(Node.ENodeType.OUTPUT, 5);

            net.addNodes(new Node[] { inNode1, inNode2, hiddenNode, outNode1, outNode2 });

            net.addLink(new Link(inNode1, hiddenNode, false, 1.0f));
            net.addLink(new Link(inNode2, hiddenNode, false, 2.0f));
            net.addLink(new Link(hiddenNode, outNode1, false, 3.0f));
            net.addLink(new Link(hiddenNode, outNode2, false, 4.0f));

            net.ActivationFunction = new TestActivationFunction();

            net.SerializeBinary("./test");

            net = Network.DeserializeNetwork("./test");

            // Check nodes
            Assert.AreEqual(5, net.Nodes.Count);
            Assert.AreEqual(2, net.Input.Count);
            Assert.AreEqual(2, net.Output.Count);
            for (int i = 0; i < 5; ++i)
                Assert.AreEqual(i + 1, net.Nodes[i].ID);

            // Check links
            Assert.AreEqual(4, net.Links.Count);
            for (int i = 0; i < 4; ++i)
            {
                Assert.AreEqual(i + 1, net.Links[i].Weight);
                Assert.AreEqual(false, net.Links[i].IsRecurrent);
            }

            Assert.AreEqual(1, net.Links[0].InNode.ID);
            Assert.AreEqual(3, net.Links[0].OutNode.ID);

            Assert.AreEqual(2, net.Links[1].InNode.ID);
            Assert.AreEqual(3, net.Links[1].OutNode.ID);

            Assert.AreEqual(3, net.Links[2].InNode.ID);
            Assert.AreEqual(4, net.Links[2].OutNode.ID);

            Assert.AreEqual(3, net.Links[3].InNode.ID);
            Assert.AreEqual(5, net.Links[3].OutNode.ID);

            Assert.IsTrue(net.ActivationFunction is TestActivationFunction);
        }
    }
}