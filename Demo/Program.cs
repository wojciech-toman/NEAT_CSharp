using System;
using System.Collections.Generic;

using NEAT_CSharp;

namespace NEAT_CSharp.Demo
{
    using NEAT_CSharp;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;


	// Basic demo program checking basic functions of the network
	class Program
	{
		static void Main(string[] args)
        {
            Random r = new Random();

            ShowMenuOptions();

            ConsoleKeyInfo key;
            while ((key = Console.ReadKey()).Key != ConsoleKey.Escape)
            {
                Console.WriteLine("");
                if (key.Key == ConsoleKey.D1)
                    XorTest(r);
                else if (key.Key == ConsoleKey.D2)
                    PoleBalanceSingleTest(r);
                else if (key.Key == ConsoleKey.D3)
                    SnakeGameTest(r);
                else if (key.Key == ConsoleKey.D4)
                    SnakeGameReplay(r);
                else
                    Console.WriteLine("Demo not found");

                Console.WriteLine("-----------------");
                ShowMenuOptions();
            }
        }

        private static void ShowMenuOptions()
        {
            Console.WriteLine("Tests of NEAT. Press:");
            Console.WriteLine("- 1 to run XOR test");
            Console.WriteLine("- 2 to run Pole Balancing (single) test");
            Console.WriteLine("- 3 to run Snake Game test (prints only results)");
            Console.WriteLine("- 4 to run Snake Game replay (using network saved in prior simulation)");
            Console.WriteLine("- ESC to quit");
        }

        private static void SnakeGameTest(Random r)
		{
			SnakeSimulation sim = new SnakeSimulation();
			sim.Start();
			// Run 250 epochs
			while (true) // sim.GenerationID < 250)
			{
				sim.Update();
			}
			Console.WriteLine("\nSimulation finished");
		}

		private static void SnakeGameReplay(Random r)
		{
            int oldWidth = Console.WindowWidth;
            int oldHeight = Console.WindowHeight;

            if (!File.Exists("./best_snake"))
			{
				Console.WriteLine("\nNo saved networks found. Please make sure to run simulation first and then try again");
				return;
			}
			Network net = Network.DeserializeNetwork("./best_snake");
			Snake snakeObj = new Snake();
			snakeObj.Brain = net;
			snakeObj.rnd = r;
			snakeObj.RunID = 0;
			snakeObj.Seed = r.Next();
			snakeObj.Start();

			// Draw the board
			Console.SetWindowSize(Snake.gridWidth + 2, Snake.gridHeight + 2);
			Console.Clear();
			for (int y = 0; y <= Snake.gridHeight; ++y)
			{
				for (int x = 0; x <= Snake.gridWidth; ++x)
				{
					if (x == 0 || x == Snake.gridWidth || y == 0 || y == Snake.gridHeight)
					{
						Console.SetCursorPosition(x, y);
						Console.Write("#");
					}
				}
			}

			Point previousSnakeLocation;
			previousSnakeLocation.x = -100;
			previousSnakeLocation.y = -100;
			List<Point> previousSnake = new List<Point>();
			List<Point> pointsToDraw = new List<Point>();

			while (!snakeObj.IsDead)
			{
				// Clear previous food and snake
				if (previousSnakeLocation.x != -100 && previousSnakeLocation.y != -100 &&
					previousSnakeLocation.x != snakeObj.FoodLocation.x && previousSnakeLocation.y != snakeObj.FoodLocation.y)
				{
					Console.SetCursorPosition(previousSnakeLocation.x + Snake.gridWidth / 2, previousSnakeLocation.y + Snake.gridHeight / 2);
					Console.Write(" ");
				}
				previousSnakeLocation = snakeObj.FoodLocation;

				pointsToDraw.Clear();
				pointsToDraw.Add(snakeObj.CurrentLocation);
				pointsToDraw.AddRange(snakeObj.Tails);

				if (previousSnake.Count > 0)
				{
					List<Point> pointsToClear = new List<Point>();
					foreach (Point p in previousSnake)
					{
						if (!pointsToDraw.Contains(p))
						{
							pointsToClear.Add(p);
							pointsToDraw.Remove(p);
						}
					}
					foreach (Point p in pointsToClear)
					{
						Console.SetCursorPosition(p.x + Snake.gridWidth / 2, p.y + Snake.gridHeight / 2);
						Console.Write(" ");
					}
				}

				previousSnake.Clear();
				previousSnake.Add(snakeObj.CurrentLocation);
				previousSnake.AddRange(snakeObj.Tails);

				// Draw current food and snake
				Console.ForegroundColor = ConsoleColor.Red;
				Console.SetCursorPosition(snakeObj.FoodLocation.x + Snake.gridWidth / 2, snakeObj.FoodLocation.y + Snake.gridHeight / 2);
				Console.Write("O");

				Console.ForegroundColor = ConsoleColor.Green;
				foreach (Point p in pointsToDraw)
				{
					Console.SetCursorPosition(p.x + Snake.gridWidth / 2, p.y + Snake.gridHeight / 2);
					Console.Write("@");
				}
				Console.ForegroundColor = ConsoleColor.White;

				Console.SetCursorPosition(0, 0);

				// Move the snake
				snakeObj.MoveOnce();

				Thread.Sleep(10);
			}


            Console.SetWindowSize(oldWidth, oldHeight);
            Console.Clear();
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
					numnodes = gen.Nodes.Count;
					thresh = numnodes * 2;

					gen.Fitness = go_cart(network, MAX_STEPS, thresh, r);
					if (gen.Fitness > epochBestFitness)
					{
						epochBestFitness = gen.Fitness;
					}
					avgConnectionGenes += gen.ConnectionGenes.Count;

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
				Console.WriteLine(String.Format("Solution network nodes count: {0} | connections count: {1}", bestGenome.Nodes.Count, bestGenome.ConnectionGenes.Count));
			else
				Console.WriteLine("Solution NOT found!");
		}

		//     cart_and_pole() was take directly from the pole simulator written
		//     by Richard Sutton and Charles Anderson.
		private static int go_cart(Network net, int max_steps, int thresh, Random rnd)
		{
			float x,                /* cart position, meters */
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
			sim.Parameters.AreConnectionWeightsCapped = false;
			sim.Parameters.MaxWeight = 1.0f;
			sim.Parameters.WeightMutationPower = 2.5f;

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
                    //network.ActivationFunction = new NeuralEvolution.ActivationFunctions.ReLU();
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

					avgConnectionGenes += gen.ConnectionGenes.Count;

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

				// We found a solution so we can exit already
				if (solutionFound)
					break;

				// Advance to the next step of simulation
				sim.epoch();
			}

			if (solutionFound)
			{
				Console.WriteLine(String.Format("Solution found: [{0}, {1}, {2}, {3}]", bestOutput[0], bestOutput[1], bestOutput[2], bestOutput[3]));
				Console.WriteLine(String.Format("Solution network nodes count: {0} | connections count: {1}", bestGenome.Nodes.Count, bestGenome.ConnectionGenes.Count));
			}
			else
				Console.WriteLine("Solution NOT found!");
		}
	}
}
