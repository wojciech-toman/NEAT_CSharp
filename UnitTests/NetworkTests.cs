﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeuralEvolution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralEvolution.Tests
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
    }
}