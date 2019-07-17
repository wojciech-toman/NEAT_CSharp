using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralEvolution
{
	// Basic demo program checking basic functions of the network
	class Program
	{
		static void Main(string[] args)
		{
			Random r = new Random();

			Console.WriteLine("Tests of NEAT. Press:");
			Console.WriteLine("- 1 to run XOR test");
			Console.WriteLine("- 2 to run Pole Balancing (single) test");
			Console.WriteLine("- 0 to perform basic system tests (mutation, innovation, etc.)");
			Console.WriteLine("- ESC to quit");

			ConsoleKeyInfo key;
			while((key = Console.ReadKey()).Key != ConsoleKey.Escape)
			{
				Console.WriteLine("");
				if (key.Key == ConsoleKey.D1)
					XorTest(r);
				else if (key.Key == ConsoleKey.D2)
					PoleBalanceSingleTest(r);
				else if (key.Key == ConsoleKey.D0)
					SystemTests(r);
				else
					Console.WriteLine("Test not found");
			}
		}

		private static void SystemTests(Random r)
		{
			///////////////////////
			// Test crossover
			///////////////////////

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

			Console.WriteLine("Genome 1:");
			gen1.debugPrint();
			Console.WriteLine("\nGenome 2:");
			gen2.debugPrint();
			Console.WriteLine("\n-- RESULT:");
			Console.WriteLine(string.Format("Compatibility: {0} | Matching: {1} | Excess: {2} | Disjoint: {3} | Weight diff: {4}", gen1.compatibilityDistance(gen2), Genome.matchingGenesCount(gen1, gen2), Genome.excessGenesCount(gen1, gen2), Genome.disjointGenesCount(gen1, gen2), Genome.getAverageWeightDifference(gen1, gen2)));
			Console.WriteLine("-- EXPECTED:");
			Console.WriteLine(string.Format("Compatibility: {0} | Matching: {1} | Excess: {2} | Disjoint: {3} | Weight diff: {4}", 5.04, 5, 2, 3, 0.1));


			Console.WriteLine("\n\nCrossover:");
			gen1.crossover(gen2, r).debugPrint();


			///////////////////////
			// Test add node mutation
			///////////////////////
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

			///////////////////////
			// Test add connection mutation
			///////////////////////
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

		private static void PoleBalanceSingleTest(Random r)
		{
			Genome startGenome = new Genome(r);
			startGenome.addNode(new Node(Node.ENodeType.SENSOR, 1));
			startGenome.addNode(new Node(Node.ENodeType.SENSOR, 2));
			startGenome.addNode(new Node(Node.ENodeType.SENSOR, 3));
			startGenome.addNode(new Node(Node.ENodeType.SENSOR, 4));
			startGenome.addNode(new Node(Node.ENodeType.SENSOR, 5));
			startGenome.addNode(new Node(Node.ENodeType.OUTPUT, 6));
			startGenome.addNode(new Node(Node.ENodeType.OUTPUT, 7));

			startGenome.addConnection(1, 6, 0.0f);
			startGenome.addConnection(2, 6, 0.0f);
			startGenome.addConnection(3, 6, 0.0f);
			startGenome.addConnection(4, 6, 0.0f);
			startGenome.addConnection(5, 6, 0.0f);
			startGenome.addConnection(1, 7, 0.0f);
			startGenome.addConnection(2, 7, 0.0f);
			startGenome.addConnection(3, 7, 0.0f);
			startGenome.addConnection(4, 7, 0.0f);
			startGenome.addConnection(5, 7, 0.0f);

			// Run simulation
			Simulation sim = new Simulation(r, startGenome, 150);
			int numberOfRuns = 200;
			int numnodes;  // Used to figure out how many nodes should be visited during activation
			int thresh;    // How many visits will be allowed before giving up (for loop detection)
			int MAX_STEPS = 100000;
			bool solutionFound = false;
			Genome bestGenome = null;

			for (int i = 0; i < numberOfRuns; ++i)
			{
				double epochBestFitness = 0.0f;
				float avgConnectionGenes = 0.0f;

				// Evaluate all genomes
				foreach (Genome gen in sim.Genomes)
				{
					Network network = gen.getNetwork();
					numnodes = gen.getNodes().Count;
					thresh = numnodes * 2;

					gen.Fitness = go_cart(network, MAX_STEPS, thresh, r);
					if (gen.Fitness > epochBestFitness)
					{
						epochBestFitness = gen.Fitness;
					}
					avgConnectionGenes += gen.getConnectionGenes().Count;

					if (gen.Fitness >= MAX_STEPS)
					{
						bestGenome = gen;
						solutionFound = true;
						break;
					}
				}

				avgConnectionGenes /= sim.Genomes.Count;

				Console.WriteLine(String.Format("Epoch {0} | best: {1} | avg genes: {2} | species: {3}", i, epochBestFitness, avgConnectionGenes, sim.Species.Count));

				if (solutionFound)
					break;

				sim.epoch();
			}

			if (solutionFound)
				Console.WriteLine(String.Format("Solution network nodes count: {0} | connections count: {1}", bestGenome.getNodes().Count, bestGenome.getConnectionGenes().Count));
			else
				Console.WriteLine("Solution NOT found!");
		}

		//     cart_and_pole() was take directly from the pole simulator written
		//     by Richard Sutton and Charles Anderson.
		private static int go_cart(Network net, int max_steps, int thresh, Random rnd)
		{
			float x,			    /* cart position, meters */
				  x_dot,            /* cart velocity */
				  theta,            /* pole angle, radians */
				  theta_dot;        /* pole angular velocity */
			int steps = 0, y;

			float[] input = new float[5];  //Input loading array

			float out1;
			float out2;

			float twelve_degrees = 0.2094384f;

			/* if (random_start) {
			   x = (lrand48()%4800)/1000.0 - 2.4;
			   x_dot = (lrand48()%2000)/1000.0 - 1;
			   theta = (lrand48()%400)/1000.0 - .2;
			   theta_dot = (lrand48()%3000)/1000.0 - 1.5;
			  }
			 else */
			//x = x_dot = theta = theta_dot = 0.0f;

			x = (rnd.Next(0, 4800) / 1000.0f - 2.4f);
			x_dot = (rnd.Next(0, 2000) / 1000.0f - 1.0f);
			theta = (rnd.Next(0, 400) / 1000.0f - 0.2f);
			theta_dot = (rnd.Next(0, 3000) / 1000.0f - 1.5f);

			/*--- Iterate through the action-learn loop. ---*/
			while (steps++ < max_steps)
			{
				/*-- setup the input layer based on the four iputs --*/
				input[0] = 1.0f;  //Bias
				input[1] = (x + 2.4f) / 4.8f;
				input[2] = (x_dot + 0.75f) / 1.5f;
				input[3] = (theta + twelve_degrees) / 0.41f;
				input[4] = (theta_dot + 1.0f) / 2.0f;
				net.setInput(input);

				// Activate the net
				// If it loops, exit returning only fitness of 1 step
				if (!(net.activate())) return 1;

				/*-- Decide which way to push via which output unit is greater --*/
				List<Node> outputs = net.Output;
				out1 = outputs[0].Activation;
				out2 = outputs[1].Activation;
				if (out1 > out2)
					y = 0;
				else
					y = 1;

				/*--- Apply action to the simulated cart-pole ---*/
				cart_pole(y, ref x, ref x_dot, ref theta, ref theta_dot);

				/*--- Check for failure.  If so, return steps ---*/
				if (x < -2.4 || x > 2.4 || theta < -twelve_degrees || theta > twelve_degrees)
					return steps;
			}

			return steps;
		}


		//     cart_and_pole() was take directly from the pole simulator written
		//     by Richard Sutton and Charles Anderson.
		//     This simulator uses normalized, continous inputs instead of 
		//    discretizing the input space.
		/*----------------------------------------------------------------------
		   cart_pole:  Takes an action (0 or 1) and the current values of the
		 four state variables and updates their values by estimating the state
		 TAU seconds later.
		----------------------------------------------------------------------*/
		static void cart_pole(int action, ref float x, ref float x_dot, ref float theta, ref float theta_dot)
		{
			float xacc, thetaacc, force, costheta, sintheta, temp;

			const float GRAVITY = 9.8f;
			const float MASSCART = 1.0f;
			const float MASSPOLE = 0.1f;
			const float TOTAL_MASS = (MASSPOLE + MASSCART);
			const float LENGTH = 0.5f;     /* actually half the pole's length */
			const float POLEMASS_LENGTH = (MASSPOLE * LENGTH);
			const float FORCE_MAG = 10.0f;
			const float TAU = 0.02f;   /* seconds between state updates */
			const float FOURTHIRDS = 1.3333333333333f;

			force = (action > 0) ? FORCE_MAG : -FORCE_MAG;
			costheta = (float)Math.Cos(theta);
			sintheta = (float)Math.Sin(theta);

			temp = (force + POLEMASS_LENGTH * theta_dot * theta_dot * sintheta)
			  / TOTAL_MASS;

			thetaacc = (GRAVITY * sintheta - costheta * temp)
			  / (LENGTH * (FOURTHIRDS - MASSPOLE * costheta * costheta
				   / TOTAL_MASS));

			xacc = temp - POLEMASS_LENGTH * thetaacc * costheta / TOTAL_MASS;

			/*** Update the four state variables, using Euler's method. ***/

			x += TAU * x_dot;
			x_dot += TAU * xacc;
			theta += TAU * theta_dot;
			theta_dot += TAU * thetaacc;
		}

		private static void XorTest(Random r)
		{
			Genome xorGene = new Genome(r);
			xorGene.addNode(new Node(Node.ENodeType.SENSOR, 1));
			xorGene.addNode(new Node(Node.ENodeType.SENSOR, 2));
			xorGene.addNode(new Node(Node.ENodeType.SENSOR, 3));
			xorGene.addNode(new Node(Node.ENodeType.OUTPUT, 4));

			xorGene.addConnection(1, 4, 0.0f);
			xorGene.addConnection(2, 4, 0.0f);
			xorGene.addConnection(3, 4, 0.0f);

			// Run simulation
			Simulation sim = new Simulation(r, xorGene, 150);
			sim.Parameters.AreConnectionWeightsCapped = true;
			sim.Parameters.MaxWeight = 1.0f;
			sim.Parameters.WeightMutationPower = 1.0f;

			int numberOfRuns = 200;
			bool solutionFound = false;
			Genome bestGenome = null;
			float[] output = { 0.0f, 0.0f, 0.0f, 0.0f };
			float[] bestOutput = new float[4];
			float[,] input = {
				{ 1.0f, 0.0f, 0.0f },
				{ 1.0f, 0.0f, 1.0f },
				{ 1.0f, 1.0f, 0.0f },
				{ 1.0f, 1.0f, 1.0f }
			};
			for (int i = 0; i < numberOfRuns; ++i)
			{
				double epochBestFitness = 0.0f;
				float avgConnectionGenes = 0.0f;

				// Evaluate all genomes
				foreach (Genome gen in sim.Genomes)
				{
					Network network = gen.getNetwork();
					bool couldActivate = true;
					for (int inputIdx = 0; inputIdx < 4; ++inputIdx)
					{
						float[] inputRow = new float[3];
						for (int k = 0; k < 3; ++k)
							inputRow[k] = input[inputIdx, k];
						network.setInput(inputRow);
						if (!network.activate())
							couldActivate = false;

						// Relax network
						int networkDepth = network.getMaxDepth();
						for (int relax = 0; relax <= networkDepth; ++relax)
						{
							network.activate();
						}

						output[inputIdx] = network.Output[0].Activation;

						network.reset();
					}

					float errorsum = (float)(Math.Abs(output[0]) + Math.Abs(1.0 - output[1]) + Math.Abs(1.0 - output[2]) + Math.Abs(output[3]));
					if (!couldActivate)
						errorsum = 4;
					gen.Fitness = Math.Pow((4.0 - errorsum), 2);
					gen.Error = errorsum;

					if (gen.Fitness > epochBestFitness)
					{
						bestOutput[0] = output[0]; bestOutput[1] = output[1]; bestOutput[2] = output[2]; bestOutput[3] = output[3];
						epochBestFitness = gen.Fitness;
					}

					avgConnectionGenes += gen.getConnectionGenes().Count;

					if ((output[0] < 0.5f) && (output[1] >= 0.5f) && (output[2] >= 0.5f) && (output[3] < 0.5f))
					{
						bestOutput[0] = output[0]; bestOutput[1] = output[1]; bestOutput[2] = output[2]; bestOutput[3] = output[3];
						bestGenome = gen;
						solutionFound = true;
						break;
					}
				}

				avgConnectionGenes /= sim.Genomes.Count;

				Console.WriteLine(String.Format("Epoch {0} | best: {1} | best output: [{2}, {3}, {4}, {5}] | avg genes: {6} | species: {7}", i, epochBestFitness, bestOutput[0], bestOutput[1], bestOutput[2], bestOutput[3], avgConnectionGenes, sim.Species.Count));

				if (solutionFound)
					break;

				sim.epoch();
			}

			if (solutionFound)
			{
				Console.WriteLine(String.Format("Solution found: [{0}, {1}, {2}, {3}]", bestOutput[0], bestOutput[1], bestOutput[2], bestOutput[3]));
				Console.WriteLine(String.Format("Solution network nodes count: {0} | connections count: {1}", bestGenome.getNodes().Count, bestGenome.getConnectionGenes().Count));
			}
			else
				Console.WriteLine("Solution NOT found!");
		}
	}
}
