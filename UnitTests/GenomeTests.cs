using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NEAT_CSharp;

namespace NEAT_CSharp.Tests
{
    [TestClass]
    public class GenomeTests
    {
        Random r = null;
        Genome gen1 = null;
        Genome gen2 = null;

        [TestInitialize]
        public void SetupTest()
        {
            r = new Random(0);

            // Genome 1
            gen1 = new Genome(r);

            // Create 3 sensors
            gen1.AddNode(new Node(Node.ENodeType.SENSOR, 1));
            gen1.AddNode(new Node(Node.ENodeType.SENSOR, 2));
            gen1.AddNode(new Node(Node.ENodeType.SENSOR, 3));

            // Create 1 output
            gen1.AddNode(new Node(Node.ENodeType.OUTPUT, 4));

            // Create 1 hidden node
            gen1.AddNode(new Node(Node.ENodeType.HIDDEN, 5));

            // Add connections from the paper
            gen1.AddConnection(1, 4, 0.5f);
            gen1.AddConnection(2, 4, false);
            gen1.AddConnection(3, 4);
            gen1.AddConnection(2, 5);
            gen1.AddConnection(5, 4);
            gen1.AddConnection(1, 5, true, 8);



            // Genome 2
            gen2 = new Genome(r);

            // Create 3 sensors
            gen2.AddNode(new Node(Node.ENodeType.SENSOR, 1));
            gen2.AddNode(new Node(Node.ENodeType.SENSOR, 2));
            gen2.AddNode(new Node(Node.ENodeType.SENSOR, 3));

            // Create 1 output
            gen2.AddNode(new Node(Node.ENodeType.OUTPUT, 4));

            // Create 2 hidden nodes
            gen2.AddNode(new Node(Node.ENodeType.HIDDEN, 5));
            gen2.AddNode(new Node(Node.ENodeType.HIDDEN, 6));

            // Add connections from the paper
            gen2.AddConnection(1, 4);
            gen2.AddConnection(2, 4, false);
            gen2.AddConnection(3, 4);
            gen2.AddConnection(2, 5);
            gen2.AddConnection(5, 4, false);
            gen2.AddConnection(5, 6);
            gen2.AddConnection(6, 4);
            gen2.AddConnection(3, 5, true, 9);
            gen2.AddConnection(1, 6, true, 10);
        }

        [TestCleanup]
        public void CleanupTest()
        {
            r = null;
            gen1 = null;
            gen2 = null;
        }


        [TestMethod]
        public void TestGenomeConstruction()
        {
            Assert.IsTrue(gen1.Nodes.Count == 5);
            Assert.IsTrue(gen1.ConnectionGenes.Count == 6);

            for (int i = 1; i <= gen1.Nodes.Count; ++i)
            {
                Assert.IsTrue(gen1.GetNodeById(i).ID == i);
            }
            Assert.IsTrue(gen1.GetNodeById(0) == null);
            Assert.IsTrue(gen1.GetNodeById(6) == null);
        }

        [TestMethod]
        public void TestGenomeConstruction_ByCopying()
        {
            Genome genCopied = gen1.Copy();


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

            gen3.AddNode(new Node(Node.ENodeType.SENSOR, 1));
            gen3.AddNode(new Node(Node.ENodeType.SENSOR, 2));
            gen3.AddNode(new Node(Node.ENodeType.OUTPUT, 3));

            gen3.AddConnection(1, 3, 0.5f);
            gen3.AddConnection(2, 3, 1.0f);

            Assert.IsTrue(gen3.Nodes.Count == 3);
            gen3.AddNodeMutation(innovations);
            Assert.IsTrue(gen3.Nodes.Count == 4);
        }

        [TestMethod]
        public void TestCrossover()
        {
            Simulation tmpSim = new Simulation(r, gen1, 1);

            gen1.ParentSimulation = tmpSim;
            gen2.ParentSimulation = tmpSim;

            float epsilon = 0.0001f;

            Assert.IsTrue(Math.Abs(gen1.CompatibilityDistance(gen2) - 5.04) < epsilon);
            Assert.AreEqual(5, Genome.MatchingGenesCount(gen1, gen2));
            Assert.AreEqual(2, Genome.ExcessGenesCount(gen1, gen2));
            Assert.AreEqual(3, Genome.DisjointGenesCount(gen1, gen2));
            Assert.IsTrue(Math.Abs(Genome.GetAverageWeightDifference(gen1, gen2) - 0.1) < epsilon);


            Console.WriteLine("\n\nCrossover:");
            gen1.Crossover(gen2, r).DebugPrint();
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
            gen4.AddNode(new Node(Node.ENodeType.SENSOR, 1));
            gen4.AddNode(new Node(Node.ENodeType.SENSOR, 2));
            gen4.AddNode(new Node(Node.ENodeType.SENSOR, 3));

            // Create 1 output
            gen4.AddNode(new Node(Node.ENodeType.OUTPUT, 4));

            // Create 1 hidden node
            gen4.AddNode(new Node(Node.ENodeType.HIDDEN, 5));

            // Add connections from the paper
            gen4.AddConnection(1, 4);
            gen4.AddConnection(2, 4);
            gen4.AddConnection(3, 4);
            gen4.AddConnection(2, 5);
            gen4.AddConnection(5, 4);

            Assert.IsTrue(gen4.ConnectionGenes.Count == 5);
            gen4.AddConnectionMutation(innovations);
            Assert.IsTrue(gen4.ConnectionGenes.Count == 6);
        }

        [TestMethod]
        public void TestToggleConnectionEnabilityMutation()
        {
            Genome gen5 = gen2.Copy();
            Assert.IsTrue(gen5.ConnectionGenes[7].IsEnabled == true);
            gen5.ToggleEnabledMutation();
            Assert.IsTrue(gen5.ConnectionGenes[7].IsEnabled == false);

            gen5 = gen2.Copy();
            Assert.IsTrue(gen5.ConnectionGenes[1].IsEnabled == false);
            gen5.ReenableMutation();

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
            // Change enability of some genes from 'false' to 'true'
            gen2.ConnectionGenes[1].IsEnabled = true;
            gen2.ConnectionGenes[4].IsEnabled = true;


            Network net = gen2.GetNetwork();


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
            // Change enability of some genes from 'true' to 'false'
            gen2.ConnectionGenes[7].IsEnabled = false;
            gen2.ConnectionGenes[8].IsEnabled = false;


            Network net = gen2.GetNetwork();


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
            gen1.AddNode(new Node(Node.ENodeType.SENSOR, 1));
            gen1.AddNode(new Node(Node.ENodeType.SENSOR, 2));
            gen1.AddNode(new Node(Node.ENodeType.SENSOR, 3));

            // Create 1 output
            gen1.AddNode(new Node(Node.ENodeType.OUTPUT, 4));

            // Create 1 hidden node
            gen1.AddNode(new Node(Node.ENodeType.HIDDEN, 5));

            // Add connections from the paper
            gen1.AddConnection(1, 4, 0.5f);
            Assert.IsTrue(gen1.GetInnovationNumber() == 1);

            gen1.AddConnection(2, 4, false);
            Assert.IsTrue(gen1.GetInnovationNumber() == 2);
            gen1.AddConnection(3, 4);
            Assert.IsTrue(gen1.GetInnovationNumber() == 3);
            gen1.AddConnection(2, 5);
            Assert.IsTrue(gen1.GetInnovationNumber() == 4);
            gen1.AddConnection(5, 4);
            Assert.IsTrue(gen1.GetInnovationNumber() == 5);
            gen1.AddConnection(1, 5, true);
            Assert.IsTrue(gen1.GetInnovationNumber() == 6);
        }

        [TestMethod]
        public void CompatibilityDistanceTest_EmptyGenomes_Expected_0()
        {
            Genome gen1 = new Genome(r);
            Genome gen2 = new Genome(r);

            Simulation tmpSim = new Simulation(r, gen1, 1);

            gen1.ParentSimulation = tmpSim;
            gen2.ParentSimulation = tmpSim;

            float epsilon = 0.0001f;
            Assert.IsTrue(gen1.CompatibilityDistance(gen2) < epsilon);
            Assert.IsTrue(gen2.CompatibilityDistance(gen1) < epsilon);
        }

        [TestMethod]
        public void CompatibilityDistanceTest_ToSelf_Expected_0()
        {
            float epsilon = 0.0001f;
            Simulation tmpSim = new Simulation(r, gen1, 1);
            gen1.ParentSimulation = tmpSim;

            Assert.IsTrue(gen1.CompatibilityDistance(gen1) < epsilon);
        }

        [TestMethod]
        public void CompatibilityDistanceTest_ToCopyOfSelf_Expected_0()
        {
            float epsilon = 0.0001f;
            Simulation tmpSim = new Simulation(r, gen1, 1);
            gen1.ParentSimulation = tmpSim;

            Genome genCopy = gen1.Copy();
            genCopy.ParentSimulation = tmpSim;

            Assert.IsTrue(gen1.CompatibilityDistance(genCopy) < epsilon);
            Assert.IsTrue(genCopy.CompatibilityDistance(gen1) < epsilon);
        }

        [TestMethod]
        public void CompatibilityDistanceTest_TwoDifferentGenomes()
        {
            Simulation tmpSim = new Simulation(r, gen1, 1);

            gen1.ParentSimulation = tmpSim;
            gen2.ParentSimulation = tmpSim;

            float epsilon = 0.0001f;

            Assert.IsTrue(Math.Abs(gen1.CompatibilityDistance(gen2) - 5.04) < epsilon);
        }

        [TestMethod]
        public void CompatibilityDistanceTest_TwoDifferentGenomes_2()
        {
            Simulation tmpSim = new Simulation(r, gen1, 1);

            Genome genCopy = gen1.Copy();

            gen1.ParentSimulation = tmpSim;
            genCopy.ParentSimulation = tmpSim;

            genCopy.AddConnection(1, 2, 0.0f);

            float epsilon = 0.0001f;

            Assert.IsTrue(Genome.DisjointGenesCount(gen1, genCopy) == 1);
            float distance = Math.Abs(gen1.CompatibilityDistance(genCopy));
            Assert.IsTrue(Math.Abs(distance - 1.0f) < epsilon, String.Format("Expected {0}, got {1}", 1.0f, distance));
        }

        [TestMethod]
        public void CompatibilityDistanceTest_TwoDifferentGenomes_3()
        {
            Simulation tmpSim = new Simulation(r, gen1, 1);

            Genome genCopy = gen1.Copy();

            gen1.ParentSimulation = tmpSim;
            genCopy.ParentSimulation = tmpSim;

            // Change some weights and add connections
            genCopy.ConnectionGenes[0].Weight = 0.0f;
            genCopy.ConnectionGenes[1].Weight = 0.0f;
            genCopy.AddConnection(1, 2, 0.0f);
            genCopy.AddConnection(1, 3, 0.0f);

            float epsilon = 0.0001f;

            Assert.IsTrue(Genome.DisjointGenesCount(gen1, genCopy) == 2);

            float distance = Math.Abs(gen1.CompatibilityDistance(genCopy));
            float weightDiff = 0.4f * (0.5f + 1.0f) / 6.0f;
            float expected = 2.0f + weightDiff; // -> 2 new genes (2 x 1.0) + average weight difference
            Assert.IsTrue(Math.Abs(distance - expected) < epsilon, String.Format("Expected {0}, got {1}", expected, distance));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CompatibilityDistanceTest_ToNull_Expected_Exception()
        {
            Simulation tmpSim = new Simulation(r, gen1, 1);
            gen1.ParentSimulation = tmpSim;

            gen1.CompatibilityDistance(null);
        }

        [TestMethod]
        public void NextInnovationNumberTest()
        {
            Simulation tmpSim = new Simulation(r, gen1, 1);
            gen1.ParentSimulation = tmpSim;

            Assert.AreEqual(6, gen1.NextInnovationNumber());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddConnectionTest_Null_ExpectectedException()
        {
            Simulation tmpSim = new Simulation(r, gen1, 1);
            gen1.ParentSimulation = tmpSim;

            gen1.AddConnection(null);
        }

        [TestMethod]
        public void AddConnectionTest_NonExisting()
        {
            Simulation tmpSim = new Simulation(r, gen1, 1);
            gen1.ParentSimulation = tmpSim;

            ConnectionGene connection = new ConnectionGene(gen1.GetNodeById(1), gen1.GetNodeById(3), false, 1.0f, true, 1);

            gen1.AddConnection(connection);

            Assert.AreEqual(7, gen1.ConnectionGenes.Count);
            Assert.AreEqual(5, gen1.Nodes.Count);
        }

        [TestMethod]
        public void AddConnectionTest_OneNonExistingNode_Expected_AddIt()
        {
            Simulation tmpSim = new Simulation(r, gen1, 1);
            gen1.ParentSimulation = tmpSim;

            ConnectionGene connection = new ConnectionGene(gen1.GetNodeById(1), new Node(Node.ENodeType.HIDDEN, 10, 0.0f), false, 1.0f, true, 1);

            gen1.AddConnection(connection);

            Assert.AreEqual(7, gen1.ConnectionGenes.Count);
            Assert.AreEqual(6, gen1.Nodes.Count);
            Assert.AreEqual(10, gen1.Nodes[gen1.Nodes.Count - 1].ID);
        }

        [TestMethod]
        public void AddConnectionTest_TwoNonExistingNode_Expected_AddThem()
        {
            Simulation tmpSim = new Simulation(r, gen1, 1);
            gen1.ParentSimulation = tmpSim;

            ConnectionGene connection = new ConnectionGene(new Node(Node.ENodeType.HIDDEN, 9, 0.0f), new Node(Node.ENodeType.HIDDEN, 10, 0.0f), false, 1.0f, true, 1);

            gen1.AddConnection(connection);

            Assert.AreEqual(7, gen1.ConnectionGenes.Count);
            Assert.AreEqual(7, gen1.Nodes.Count);
            Assert.AreEqual(9, gen1.Nodes[gen1.Nodes.Count - 2].ID);
            Assert.AreEqual(10, gen1.Nodes[gen1.Nodes.Count - 1].ID);
        }

        [TestMethod()]
        public void GetMoreAndLessFitTest_SameFitness()
        {
            gen1.OriginalFitness = 1.0f;
            gen2.OriginalFitness = 1.0f;

            Genome moreFit, lessFit;
            Genome.GetMoreAndLessFit(gen2, gen1, out moreFit, out lessFit);
            Assert.AreEqual(gen1, moreFit);
            Assert.AreEqual(gen2, lessFit);
        }

        [TestMethod()]
        public void GetMoreAndLessFitTest_DifferentFitness()
        {
            gen1.OriginalFitness = 1.0f;
            gen2.OriginalFitness = 2.0f;

            Genome moreFit, lessFit;
            Genome.GetMoreAndLessFit(gen2, gen1, out moreFit, out lessFit);
            Assert.AreEqual(gen2, moreFit);
            Assert.AreEqual(gen1, lessFit);
        }

        [TestMethod()]
        public void FindConnectionGeneToSplitTest_NoGenes_Expected_False()
        {
            Genome genome = new Genome(r);
            genome.AddNode(new Node(Node.ENodeType.SENSOR, 1));
            genome.AddNode(new Node(Node.ENodeType.OUTPUT, 2));

            int connectionIndex = -1;
            bool found = genome.FindConnectionGeneToSplit(out connectionIndex);

            Assert.AreEqual(false, found);
            Assert.AreEqual(-1, connectionIndex);
        }

        [TestMethod()]
        public void FindConnectionGeneToSplitTest_NoEnabledGenes_Expected_False()
        {
            Genome genome = new Genome(r);
            genome.AddNode(new Node(Node.ENodeType.SENSOR, 1));
            genome.AddNode(new Node(Node.ENodeType.OUTPUT, 2));

            genome.AddConnection(1, 2, false);

            int connectionIndex = -1;
            bool found = genome.FindConnectionGeneToSplit(out connectionIndex);

            Assert.AreEqual(false, found);
            Assert.AreEqual(-1, connectionIndex);
        }

        [TestMethod()]
        public void FindConnectionGeneToSplitTest_Correct_Expected_True()
        {
            UnitTests.RandomStub randomStub = new UnitTests.RandomStub();
            randomStub.NextIntValue = 0; randomStub.NextDoubleValue = 0.0;

            Genome genome = new Genome(randomStub);
            genome.AddNode(new Node(Node.ENodeType.SENSOR, 1));
            genome.AddNode(new Node(Node.ENodeType.OUTPUT, 2));

            genome.AddConnection(1, 2, true);

            int connectionIndex = -1;
            bool found = genome.FindConnectionGeneToSplit(out connectionIndex);

            Assert.AreEqual(true, found);
            Assert.AreEqual(0, connectionIndex);
        }
    }
}
