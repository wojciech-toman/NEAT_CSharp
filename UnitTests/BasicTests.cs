using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NeuralEvolution;

namespace NeuralEvolution.Tests
{
	[TestClass()]
	public class BasicTests
	{
		[TestMethod()]
		public void TestAddNodeMutation()
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

			Console.WriteLine("\n\nBefore node mutation:");
			gen3.debugPrint();
			Console.WriteLine("\nAfter node mutation:");
			gen3.addNodeMutation(innovations);
			gen3.debugPrint();
		}

		[TestMethod()]
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

		[TestMethod()]
		public void TestAddConnectionMutation()
		{
			Random r = new Random();

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

			Console.WriteLine("\n\nBefore connection mutation:");
			gen4.debugPrint();
			Console.WriteLine("\nAfter connection mutation:");
			gen4.addConnectionMutation(innovations);
			gen4.debugPrint();
		}

		[TestMethod()]
		public void TestToggleConnectionEnabilityMutation()
		{
			Random r = new Random();

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


			///////////////////////
			// Test toggle enabled/reanable connection mutation
			///////////////////////
			Genome gen5 = gen2.copy();
			Console.WriteLine("\n\nBefore toggle enabled mutation:");
			gen5.debugPrint();
			Console.WriteLine("\nAfter toggle enabled mutation:");
			gen5.toggleEnabledMutation();
			gen5.debugPrint();

			gen5 = gen2.copy();
			Console.WriteLine("\n\nBefore reenable connection mutation:");
			gen5.debugPrint();
			Console.WriteLine("\nAfter reenable connection mutation:");
			gen5.reenableMutation();
			gen5.debugPrint();
		}
	}
}
