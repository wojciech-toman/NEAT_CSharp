using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NeuralEvolution;

namespace NeuralEvolution.Tests
{
    [TestClass]
    public class GenomeTests
    {
        Random r = null;
        Genome gen1 = null;

        [TestInitialize]
        public void SetupTest()
        {
            r = new Random(0);

            // Genome 
            gen1 = new Genome(r);

            // Create 3 sensors
            gen1.addNode(new Node(Node.ENodeType.SENSOR, 1));
            gen1.addNode(new Node(Node.ENodeType.SENSOR, 2));
            gen1.addNode(new Node(Node.ENodeType.SENSOR, 3));

            // Create 1 output
            gen1.addNode(new Node(Node.ENodeType.OUTPUT, 4));

            // Create 1 hidden node
            gen1.addNode(new Node(Node.ENodeType.HIDDEN, 5));

            // Add connections from the paper
            gen1.addConnection(1, 4, 0.5f);
            gen1.addConnection(2, 4, false);
            gen1.addConnection(3, 4);
            gen1.addConnection(2, 5);
            gen1.addConnection(5, 4);
            gen1.addConnection(1, 5, true, 8);
        }

        [TestCleanup]
        public void CleanupTest()
        {
            r = null;
            gen1 = null;
        }


        [TestMethod]
        public void TestGenomeConstruction()
        {
            Assert.IsTrue(gen1.Nodes.Count == 5);
            Assert.IsTrue(gen1.ConnectionGenes.Count == 6);

            for (int i = 1; i <= gen1.Nodes.Count; ++i)
            {
                Assert.IsTrue(gen1.getNodeById(i).ID == i);
            }
            Assert.IsTrue(gen1.getNodeById(0) == null);
            Assert.IsTrue(gen1.getNodeById(6) == null);
        }

        [TestMethod]
        public void TestGenomeConstruction_ByCopying()
        {
            Genome genCopied = gen1.copy();


            Assert.IsTrue(genCopied.Nodes.Count == gen1.Nodes.Count);
            Assert.IsTrue(genCopied.ConnectionGenes.Count == gen1.ConnectionGenes.Count);
            for (int i = 0; i < genCopied.Nodes.Count; ++i)
            {
                Assert.IsTrue(genCopied.Nodes[i].Equals(gen1.Nodes[i]));
                Assert.AreNotSame(genCopied.Nodes[i], gen1.Nodes[i]);
            }

            for (int i = 0; i < genCopied.ConnectionGenes.Count; ++i)
            {
                Assert.IsTrue(genCopied.ConnectionGenes[i].Equals(gen1.ConnectionGenes[i]));
                Assert.AreNotSame(genCopied.ConnectionGenes[i], gen1.ConnectionGenes[i]);
            }
        }

        [TestMethod]
        public void TestAddNodeMutation()
        {
            Simulation tmpSim = new Simulation(r, gen1, 1);

            gen1.ParentSimulation = tmpSim;


            List<Innovation> innovations = new List<Innovation>();
            Genome gen3 = new Genome(r);
            gen3.ParentSimulation = tmpSim;

            gen3.addNode(new Node(Node.ENodeType.SENSOR, 1));
            gen3.addNode(new Node(Node.ENodeType.SENSOR, 2));
            gen3.addNode(new Node(Node.ENodeType.OUTPUT, 3));

            gen3.addConnection(1, 3, 0.5f);
            gen3.addConnection(2, 3, 1.0f);

            Assert.IsTrue(gen3.Nodes.Count == 3);
            gen3.addNodeMutation(innovations);
            Assert.IsTrue(gen3.Nodes.Count == 4);
        }

        [TestMethod]
        public void TestCrossover()
        {
            Genome gen2 = new Genome(r);

            // Genome 2

            // Create 3 sensors
            gen2.addNode(new Node(Node.ENodeType.SENSOR, 1));
            gen2.addNode(new Node(Node.ENodeType.SENSOR, 2));
            gen2.addNode(new Node(Node.ENodeType.SENSOR, 3));

            // Create 1 output
            gen2.addNode(new Node(Node.ENodeType.OUTPUT, 4));

            // Create 2 hidden nodes
            gen2.addNode(new Node(Node.ENodeType.HIDDEN, 5));
            gen2.addNode(new Node(Node.ENodeType.HIDDEN, 6));

            // Add connections from the paper
            gen2.addConnection(1, 4);
            gen2.addConnection(2, 4, false);
            gen2.addConnection(3, 4);
            gen2.addConnection(2, 5);
            gen2.addConnection(5, 4, false);
            gen2.addConnection(5, 6);
            gen2.addConnection(6, 4);
            gen2.addConnection(3, 5, true, 9);
            gen2.addConnection(1, 6, true, 10);


            Simulation tmpSim = new Simulation(r, gen1, 1);

            gen1.ParentSimulation = tmpSim;
            gen2.ParentSimulation = tmpSim;

            float epsilon = 0.0001f;

            Assert.IsTrue(Math.Abs(gen1.compatibilityDistance(gen2) - 5.04) < epsilon);
            Assert.AreEqual(Genome.matchingGenesCount(gen1, gen2), 5);
            Assert.AreEqual(Genome.excessGenesCount(gen1, gen2), 2);
            Assert.AreEqual(Genome.disjointGenesCount(gen1, gen2), 3);
            Assert.IsTrue(Math.Abs(Genome.getAverageWeightDifference(gen1, gen2) - 0.1) < epsilon);


            Console.WriteLine("\n\nCrossover:");
            gen1.crossover(gen2, r).debugPrint();
        }

        [TestMethod]
        public void TestAddConnectionMutation()
        {
            List<Innovation> innovations = new List<Innovation>();


            Simulation tmpSim = new Simulation(r, gen1, 1);

            gen1.ParentSimulation = tmpSim;


            Genome gen4 = new Genome(r);
            gen4.ParentSimulation = tmpSim;

            // Create 3 sensors
            gen4.addNode(new Node(Node.ENodeType.SENSOR, 1));
            gen4.addNode(new Node(Node.ENodeType.SENSOR, 2));
            gen4.addNode(new Node(Node.ENodeType.SENSOR, 3));

            // Create 1 output
            gen4.addNode(new Node(Node.ENodeType.OUTPUT, 4));

            // Create 1 hidden node
            gen4.addNode(new Node(Node.ENodeType.HIDDEN, 5));

            // Add connections from the paper
            gen4.addConnection(1, 4);
            gen4.addConnection(2, 4);
            gen4.addConnection(3, 4);
            gen4.addConnection(2, 5);
            gen4.addConnection(5, 4);

            Assert.IsTrue(gen4.ConnectionGenes.Count == 5);
            gen4.addConnectionMutation(innovations);
            Assert.IsTrue(gen4.ConnectionGenes.Count == 6);
        }

        [TestMethod]
        public void TestToggleConnectionEnabilityMutation()
        {
            // Genome 2
            Genome gen2 = new Genome(r);

            // Create 3 sensors
            gen2.addNode(new Node(Node.ENodeType.SENSOR, 1));
            gen2.addNode(new Node(Node.ENodeType.SENSOR, 2));
            gen2.addNode(new Node(Node.ENodeType.SENSOR, 3));

            // Create 1 output
            gen2.addNode(new Node(Node.ENodeType.OUTPUT, 4));

            // Create 2 hidden nodes
            gen2.addNode(new Node(Node.ENodeType.HIDDEN, 5));
            gen2.addNode(new Node(Node.ENodeType.HIDDEN, 6));

            // Add connections from the paper
            gen2.addConnection(1, 4);
            gen2.addConnection(2, 4, false);
            gen2.addConnection(3, 4);
            gen2.addConnection(2, 5);
            gen2.addConnection(5, 4, false);
            gen2.addConnection(5, 6);
            gen2.addConnection(6, 4);
            gen2.addConnection(3, 5, true, 9);
            gen2.addConnection(1, 6, true, 10);


            Genome gen5 = gen2.copy();
            Assert.IsTrue(gen5.ConnectionGenes[7].IsEnabled == true);
            gen5.toggleEnabledMutation();
            Assert.IsTrue(gen5.ConnectionGenes[7].IsEnabled == false);

            gen5 = gen2.copy();
            Assert.IsTrue(gen5.ConnectionGenes[1].IsEnabled == false);
            gen5.reenableMutation();

            Assert.IsTrue(gen5.ConnectionGenes[1].IsEnabled == true);
        }

        [TestMethod]
        public void TestLinkConstruction()
        {
            Node node1 = new Node(Node.ENodeType.SENSOR, 1);
            Node node2 = new Node(Node.ENodeType.SENSOR, 2);

            // Test 1
            Link lnk = new Link(node1, node2, false, 1.0f);

            Assert.IsTrue(lnk.InNode.ID == node1.ID);
            Assert.IsTrue(lnk.OutNode.ID == node2.ID);
            Assert.IsTrue(lnk.IsRecurrent == false);
            Assert.IsTrue(lnk.Weight == 1.0f);

            // Test 2
            lnk = new Link(node1, node2, true, 2.0f);

            Assert.IsTrue(lnk.IsRecurrent == true);
            Assert.IsTrue(lnk.Weight == 2.0f);
        }

        [TestMethod]
        public void GetNetworkFromGenome_AllGenesEnabled()
        {
            // Genome 2
            Genome gen2 = new Genome(r);

            // Create 3 sensors
            gen2.addNode(new Node(Node.ENodeType.SENSOR, 1));
            gen2.addNode(new Node(Node.ENodeType.SENSOR, 2));
            gen2.addNode(new Node(Node.ENodeType.SENSOR, 3));

            // Create 1 output
            gen2.addNode(new Node(Node.ENodeType.OUTPUT, 4));

            // Create 2 hidden nodes
            gen2.addNode(new Node(Node.ENodeType.HIDDEN, 5));
            gen2.addNode(new Node(Node.ENodeType.HIDDEN, 6));

            // Add connections from the paper
            gen2.addConnection(1, 4);
            gen2.addConnection(2, 4, true);
            gen2.addConnection(3, 4);
            gen2.addConnection(2, 5);
            gen2.addConnection(5, 4, true);
            gen2.addConnection(5, 6);
            gen2.addConnection(6, 4);
            gen2.addConnection(3, 5, true, 9);
            gen2.addConnection(1, 6, true, 10);

            Network net = gen2.getNetwork();


            Assert.IsTrue(net.Nodes.Count == gen2.Nodes.Count);
            Assert.IsTrue(net.Input.Count == 3);
            Assert.IsTrue(net.Output.Count == 1);
            Assert.IsTrue(net.Links.Count == gen2.ConnectionGenes.Count);
            for (int i = 0; i < net.Nodes.Count; ++i)
                Assert.IsTrue(net.Nodes[i].ID == gen2.Nodes[i].ID);
            for (int i = 0; i < net.Links.Count; ++i)
            {
                Assert.IsTrue(net.Links[i].InNode.ID == gen2.ConnectionGenes[i].InNodeGene.ID);
                Assert.IsTrue(net.Links[i].OutNode.ID == gen2.ConnectionGenes[i].OutNodeGene.ID);
            }
        }

        [TestMethod]
        public void GetNetworkFromGenome_SomeGenesDisabled()
        {
            // Genome 2
            Genome gen2 = new Genome(r);

            // Create 3 sensors
            gen2.addNode(new Node(Node.ENodeType.SENSOR, 1));
            gen2.addNode(new Node(Node.ENodeType.SENSOR, 2));
            gen2.addNode(new Node(Node.ENodeType.SENSOR, 3));

            // Create 1 output
            gen2.addNode(new Node(Node.ENodeType.OUTPUT, 4));

            // Create 2 hidden nodes
            gen2.addNode(new Node(Node.ENodeType.HIDDEN, 5));
            gen2.addNode(new Node(Node.ENodeType.HIDDEN, 6));

            // Add connections from the paper
            gen2.addConnection(1, 4);
            gen2.addConnection(2, 4, false);
            gen2.addConnection(3, 4);
            gen2.addConnection(2, 5);
            gen2.addConnection(5, 4, false);
            gen2.addConnection(5, 6);
            gen2.addConnection(6, 4);
            gen2.addConnection(3, 5, false);
            gen2.addConnection(1, 6, false);

            Network net = gen2.getNetwork();


            Assert.IsTrue(net.Nodes.Count == gen2.Nodes.Count);
            Assert.IsTrue(net.Input.Count == 3);
            Assert.IsTrue(net.Output.Count == 1);
            Assert.IsTrue(net.Links.Count == gen2.ConnectionGenes.Count - 4);
            for (int i = 0; i < net.Nodes.Count; ++i)
                Assert.IsTrue(net.Nodes[i].ID == gen2.Nodes[i].ID);
            int linkID = 0;
            for (int geneID = 0; geneID < gen2.ConnectionGenes.Count; ++geneID)
            {
                if (!gen2.ConnectionGenes[geneID].IsEnabled) continue;

                Assert.IsTrue(net.Links[linkID].InNode.ID == gen2.ConnectionGenes[geneID].InNodeGene.ID);
                Assert.IsTrue(net.Links[linkID].OutNode.ID == gen2.ConnectionGenes[geneID].OutNodeGene.ID);

                ++linkID;
            }
        }

        [TestMethod]
        public void InnovationNumberTest()
        {
            // Genome 
            Genome gen1 = new Genome(r);

            Assert.IsTrue(gen1.GetInnovationNumber() == 0);

            // Create 3 sensors
            gen1.addNode(new Node(Node.ENodeType.SENSOR, 1));
            gen1.addNode(new Node(Node.ENodeType.SENSOR, 2));
            gen1.addNode(new Node(Node.ENodeType.SENSOR, 3));

            // Create 1 output
            gen1.addNode(new Node(Node.ENodeType.OUTPUT, 4));

            // Create 1 hidden node
            gen1.addNode(new Node(Node.ENodeType.HIDDEN, 5));

            // Add connections from the paper
            gen1.addConnection(1, 4, 0.5f);
            Assert.IsTrue(gen1.GetInnovationNumber() == 1);

            gen1.addConnection(2, 4, false);
            Assert.IsTrue(gen1.GetInnovationNumber() == 2);
            gen1.addConnection(3, 4);
            Assert.IsTrue(gen1.GetInnovationNumber() == 3);
            gen1.addConnection(2, 5);
            Assert.IsTrue(gen1.GetInnovationNumber() == 4);
            gen1.addConnection(5, 4);
            Assert.IsTrue(gen1.GetInnovationNumber() == 5);
            gen1.addConnection(1, 5, true);
            Assert.IsTrue(gen1.GetInnovationNumber() == 6);
        }
    }
}
