using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NeuralEvolution;

namespace NeuralEvolution.Tests
{
    [TestClass]
    public class BasicTests
    {
        [TestMethod]
        public void TestGenomeConstruction()
        {
            Random r = new Random();

            // Genome 1
            Genome gen1 = new Genome(r);

            // Create 3 sensors
            gen1.addNode(new Node(Node.ENodeType.SENSOR, 1));
            gen1.addNode(new Node(Node.ENodeType.SENSOR, 2));
            gen1.addNode(new Node(Node.ENodeType.SENSOR, 3));

            // Create 1 output
            gen1.addNode(new Node(Node.ENodeType.OUTPUT, 4));

            // Create 1 hidden node
            gen1.addNode(new Node(Node.ENodeType.HIDDEN, 5));


            Assert.IsTrue(gen1.getNodes().Count == 5);
            Assert.IsTrue(gen1.getConnectionGenes().Count == 0);


            // Add connections from the paper
            gen1.addConnection(1, 4, 0.5f);
            gen1.addConnection(2, 4, false);
            gen1.addConnection(3, 4);
            gen1.addConnection(2, 5);
            gen1.addConnection(5, 4);
            gen1.addConnection(1, 5, true, 8);


            Assert.IsTrue(gen1.getNodes().Count == 5);
            Assert.IsTrue(gen1.getConnectionGenes().Count == 6);

            for (int i = 1; i <= gen1.getNodes().Count; ++i)
            {
                Assert.IsTrue(gen1.getNode(i).ID == i);
            }
            Assert.IsTrue(gen1.getNode(0) == null);
            Assert.IsTrue(gen1.getNode(6) == null);
        }

        [TestMethod]
        public void TestAddNodeMutation()
        {
            Random r = new Random(0);

            // Genome 1
            Genome gen1 = new Genome(r);

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

            Assert.IsTrue(gen3.getNodes().Count == 3);
            //Console.WriteLine("\n\nBefore node mutation:");
            //gen3.debugPrint();
            //Console.WriteLine("\nAfter node mutation:");
            gen3.addNodeMutation(innovations);
            //gen3.debugPrint();
            Assert.IsTrue(gen3.getNodes().Count == 4);
        }

        [TestMethod]
        public void TestCrossover()
        {
            Random r = new Random();

            Genome gen1 = new Genome(r);
            Genome gen2 = new Genome(r);
            // Genome 1

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
            Random r = new Random(0);

            List<Innovation> innovations = new List<Innovation>();

            // Genome 1
            Genome gen1 = new Genome(r);

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

            Assert.IsTrue(gen4.getConnectionGenes().Count == 5);
            //Console.WriteLine("\n\nBefore connection mutation:");
            //gen4.debugPrint();
            //Console.WriteLine("\nAfter connection mutation:");
            gen4.addConnectionMutation(innovations);
            Assert.IsTrue(gen4.getConnectionGenes().Count == 6);
            //gen4.debugPrint();
        }

        [TestMethod]
        public void TestToggleConnectionEnabilityMutation()
        {
            Random r = new Random(0);

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
            Assert.IsTrue(gen5.getConnectionGenes()[7].IsEnabled == true);
            /*Console.WriteLine("\n\nBefore toggle enabled mutation:");
			gen5.debugPrint();
			Console.WriteLine("\nAfter toggle enabled mutation:");*/
            gen5.toggleEnabledMutation();
            //gen5.debugPrint();
            Assert.IsTrue(gen5.getConnectionGenes()[7].IsEnabled == false);

            gen5 = gen2.copy();
            Assert.IsTrue(gen5.getConnectionGenes()[1].IsEnabled == false);
            /*Console.WriteLine("\n\nBefore reenable connection mutation:");
			gen5.debugPrint();
			Console.WriteLine("\nAfter reenable connection mutation:");*/
            gen5.reenableMutation();
            //gen5.debugPrint();

            Assert.IsTrue(gen5.getConnectionGenes()[1].IsEnabled == true);
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
            Random r = new Random(0);

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


            Assert.IsTrue(net.Nodes.Count == gen2.getNodes().Count);
            Assert.IsTrue(net.Input.Count == 3);
            Assert.IsTrue(net.Output.Count == 1);
            Assert.IsTrue(net.Links.Count == gen2.getConnectionGenes().Count);
            for (int i = 0; i < net.Nodes.Count; ++i)
                Assert.IsTrue(net.Nodes[i].ID == gen2.getNodes()[i].ID);
            for (int i = 0; i < net.Links.Count; ++i)
            {
                Assert.IsTrue(net.Links[i].InNode.ID == gen2.getConnectionGenes()[i].InNodeGene.ID);
                Assert.IsTrue(net.Links[i].OutNode.ID == gen2.getConnectionGenes()[i].OutNodeGene.ID);
            }
        }

        [TestMethod]
        public void GetNetworkFromGenome_SomeGenesDisabled()
        {
            Random r = new Random(0);

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


            Assert.IsTrue(net.Nodes.Count == gen2.getNodes().Count);
            Assert.IsTrue(net.Input.Count == 3);
            Assert.IsTrue(net.Output.Count == 1);
            Assert.IsTrue(net.Links.Count == gen2.getConnectionGenes().Count - 4);
            for (int i = 0; i < net.Nodes.Count; ++i)
                Assert.IsTrue(net.Nodes[i].ID == gen2.getNodes()[i].ID);
            int linkID = 0;
            for (int geneID = 0; geneID < gen2.getConnectionGenes().Count; ++geneID)
            {
                if(!gen2.getConnectionGenes()[geneID].IsEnabled) continue;

                Assert.IsTrue(net.Links[linkID].InNode.ID == gen2.getConnectionGenes()[geneID].InNodeGene.ID);
                Assert.IsTrue(net.Links[linkID].OutNode.ID == gen2.getConnectionGenes()[geneID].OutNodeGene.ID);

                ++linkID;
            }
        }
    }
}
