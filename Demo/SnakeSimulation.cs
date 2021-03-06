﻿namespace NEAT_CSharp.Demo
{
    using NEAT_CSharp;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    public class SnakeSimulation
	{
		#region Variables

		// Grid dimensions
		public const int gridWidth = 30;
		public const int gridHeight = 30;

		private Simulation sim;
		private System.Random rnd = null;
		private bool useNonRandomSeed = false;

		private bool saveAllBestSnakes = false;

		private int numberOfRuns = 1;
		private int run = 0;

		private List<float> bestFitnesses = new List<float>();
        private List<float> avgFitnesses = new List<float>();

        private float snakeBestAvg = 0.0f;

        private int generationID = 1;
        private List<Snake> allSnakes = new List<Snake>();

        private bool firstTime = true;
        private bool isSimulationStarted = false;

        private double bestFitnessEver = -1.0f;
		private double bestScore = -1;

		private int slowestSpeed = 20;
		private int replaySpeed = 19;

        #endregion


		public SnakeSimulation()
		{
			rnd = useNonRandomSeed ? new Random(9) : new Random();
		}

        // Use this for initialization
        public void Start()
		{
			this.InitSimulation();

			this.isSimulationStarted = true;
		}

        public void ReplaySimulation()
        {
            int oldWidth = Console.WindowWidth;
            int oldHeight = Console.WindowHeight;

            if (!File.Exists("./best_snake"))
            {
                Console.WriteLine("\nNo saved networks found. Please make sure to run simulation first and then try again");
                return;
            }
            Network net = Network.LoadNetworkFromFile("./best_snake");
            Snake snakeObj = new Snake();

            snakeObj.Brain = net;
            snakeObj.rnd = rnd;
            snakeObj.RunID = 0;
            snakeObj.Seed = this.useNonRandomSeed ? 9 : rnd.Next();
            snakeObj.Start();

            // Draw the board
            Console.SetWindowSize(SnakeSimulation.gridWidth + 2, SnakeSimulation.gridHeight + 2 + 13);
            Console.Clear();
			int y = 0;
            for (y = 0; y <= SnakeSimulation.gridHeight; ++y)
            {
                for (int x = 0; x <= SnakeSimulation.gridWidth; ++x)
                {
                    if (x == 0 || x == SnakeSimulation.gridWidth || y == 0 || y == SnakeSimulation.gridHeight)
                    {
                        Console.SetCursorPosition(x, y);
                        Console.Write("#");
                    }
                }
            }

			y = SnakeSimulation.gridHeight + 2;
			string[] captions = { "SCORE:", "SPEED:", "WALL L:", "WALL S:", "WALL R:", "TAIL L:", "TAIL S:", "TAIL R:", "FOOD L:", "FOOD S:", "FOOD R:" };
			Console.SetCursorPosition(0, y++);
			Console.Write(captions[0]);
			y++;
			for (int i = 1; i < captions.Length; ++i)
			{
				Console.SetCursorPosition(0, y++);
				Console.Write(captions[i]);
			}

            Point previousSnakeLocation;
            previousSnakeLocation.x = -100;
            previousSnakeLocation.y = -100;
            List<Point> previousSnake = new List<Point>();
            List<Point> pointsToDraw = new List<Point>();

			bool cancelled = false;

            while (!snakeObj.IsDead && !cancelled)
            {
                // Clear previous food and snake
                if (previousSnakeLocation.x != -100 && previousSnakeLocation.y != -100 &&
                    previousSnakeLocation.x != snakeObj.FoodLocation.x && previousSnakeLocation.y != snakeObj.FoodLocation.y)
                {
                    Console.SetCursorPosition(previousSnakeLocation.x + SnakeSimulation.gridWidth / 2, previousSnakeLocation.y + SnakeSimulation.gridHeight / 2);
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
                        Console.SetCursorPosition(p.x + SnakeSimulation.gridWidth / 2, p.y + SnakeSimulation.gridHeight / 2);
                        Console.Write(" ");
                    }
                }

                previousSnake.Clear();
                previousSnake.Add(snakeObj.CurrentLocation);
                previousSnake.AddRange(snakeObj.Tails);

                // Draw current food and snake
                Console.ForegroundColor = ConsoleColor.Red;
                Console.SetCursorPosition(snakeObj.FoodLocation.x + SnakeSimulation.gridWidth / 2, snakeObj.FoodLocation.y + SnakeSimulation.gridHeight / 2);
                Console.Write("O");

                Console.ForegroundColor = ConsoleColor.Green;
                foreach (Point p in pointsToDraw)
                {
                    Console.SetCursorPosition(p.x + SnakeSimulation.gridWidth / 2, p.y + SnakeSimulation.gridHeight / 2);
                    Console.Write("@");
                }
                Console.ForegroundColor = ConsoleColor.White;

				Console.SetCursorPosition(0, 0);

                // Move the snake
                snakeObj.MoveOnce();

				Console.ForegroundColor = ConsoleColor.Green;
				y = SnakeSimulation.gridHeight + 2;
				if (snakeObj.PreviousInput != null)
				{
					for (int i = 0; i < captions.Length; ++i)
					{
						Console.SetCursorPosition(10, y);
						Console.Write("            ");
						Console.SetCursorPosition(10, y++);

						if (i == 0) { Console.Write(snakeObj.Tails.Count + 1); y++; }
						else if (i == 1) Console.Write(slowestSpeed - replaySpeed + 1);
						else Console.Write(snakeObj.PreviousInput[i - 2]);
					}
				}

				Console.ForegroundColor = ConsoleColor.White;
				Console.SetCursorPosition(0, 0);

				if(Console.KeyAvailable == true)
				{
					var key = Console.ReadKey().Key;
					if (key == ConsoleKey.OemMinus) { ++replaySpeed; if (replaySpeed > slowestSpeed) replaySpeed = slowestSpeed; }
					else if (key == ConsoleKey.OemPlus) { --replaySpeed; if (replaySpeed < 1) replaySpeed = 1; }
					else if (key == ConsoleKey.Escape)
						cancelled = true;
				}

				Thread.Sleep(10 * replaySpeed);
			}


            Console.SetWindowSize(oldWidth, oldHeight);
            Console.Clear();
        }

		private void InitSimulation()
		{
			Console.WriteLine("Would you like to save all best snakes? [Yn]");
			var key = Console.ReadKey().Key;
			if (key == ConsoleKey.Y) this.saveAllBestSnakes = true;
			Console.WriteLine("");
			Console.WriteLine("--- Simulation started ---");

			Snake.StartingLength = 1;// 3;// int.Parse(this.startingSnakeLength.text);

			Genome startGenome = new Genome(rnd);
			int startIdx = 1;
			int bias = 0;           // 1 node is used as a bias (always 1.0)
			// 3 nodes are used to store distance to the walls
			// 3 nodes are used to store distance to the snake itself (to avoid collisions with self),
			// 3 nodes are used to store distance to food
			int inputs = 3 + 3 + 3;
			int hidden = 0;
			int outputs = 3;		// 3 outputs. Direction of movement - left, right, straight
			for (int i = 0; i < bias; ++i)
				startGenome.AddNode(new Node(Node.ENodeType.BIAS, startIdx++));

			int inputStart = startIdx;
            for (int i = 0; i < inputs; ++i)
				startGenome.AddNode(new Node(Node.ENodeType.SENSOR, startIdx++));

			int hiddenStart = startIdx;
			for(int i = 0; i < hidden; ++i)
				startGenome.AddNode(new Node(Node.ENodeType.HIDDEN, startIdx++));

			// Output: Move left, straight, right
			int outputStart = startIdx;
			for(int i = 0; i < outputs; ++i)
				startGenome.AddNode(new Node(Node.ENodeType.OUTPUT, startIdx++));

			if (hidden > 0)
			{
				for (int i = 0; i < hidden; ++i)
				{
					for (int j = 0; j < inputs; ++j)
						startGenome.AddConnectionGene(inputStart + j, hiddenStart + i, 0.0f);
					for (int j = 0; j < outputs; ++j)
						startGenome.AddConnectionGene(hiddenStart + i, outputStart + j, 0.0f);
				}
			}
			else
			{
				// Walls distance
				/*startGenome.AddConnectionGene(2, outputStart + 0, 0.0f);
				startGenome.AddConnectionGene(3, outputStart + 1, 0.0f);
				startGenome.AddConnectionGene(4, outputStart + 2, 0.0f);

				// Tails distance
				startGenome.AddConnectionGene(5, outputStart + 0, 0.0f);
				startGenome.AddConnectionGene(6, outputStart + 1, 0.0f);
				startGenome.AddConnectionGene(7, outputStart + 2, 0.0f);

				// Food distance
				startGenome.AddConnectionGene(8, outputStart + 0, 0.0f);
				startGenome.AddConnectionGene(9, outputStart + 1, 0.0f);
				startGenome.AddConnectionGene(10, outputStart + 2, 0.0f);*/
				for(int i = 0; i < inputs; ++i)
				{
					//for(int j = 0; j < outputs; ++j)
						//startGenome.AddConnectionGene(inputStart + i, outputStart + j, 0.0f);
					startGenome.AddConnectionGene(inputStart + i, outputStart + rnd.Next(outputs), 0.0f);
				}
			}

			// Connect bias node to every output
			for(int i = 0; i < bias; ++i)
			{
				for (int j = 0; j < outputs; ++j)
					startGenome.AddConnectionGene(i + 1, outputStart + j, 0.0f);
			}


			// Set up simulation - but don't start it
			sim = new Simulation(rnd, startGenome, 15000);
			sim.Parameters.AddNodeProbability = 0.02f;
			sim.Parameters.AddConnectionProbability = 0.1f;
			sim.Parameters.WeightMutationPower = 1.0f;
			sim.Parameters.MutateConnectionWeightsProbability = 0.8f;
			sim.Parameters.MutateToggleEnabledProbability = 0.05f;
			sim.Parameters.MutateReenableProbability = 0.025f;
			sim.Parameters.SurvivalThreshold = 0.01f;
			sim.Parameters.AreConnectionWeightsCapped = true;
			sim.Parameters.MaxWeight = 1.0f;
			sim.Parameters.MaxSpeciesGenerationsWithoutImprovement = 70;
			sim.Parameters.MaxGeneralGenerationsWithoutImprovement = 80;
		}

		// Update is called once per frame
		public void Update()
		{
			if (isSimulationStarted)
				Simulate();
		}

		private static void SnakeMoveCallback(Snake snake, CountdownEvent evt)
		{
			snake.Move();
			evt.Signal();
		}

		public int GenerationID { get => generationID; }

		void Simulate()
		{
			// Print the best result so far
			if (bestFitnessEver > bestScore)
			{
				Console.WriteLine("");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine(System.String.Format("Generation: {0} | Current best: {1}", this.generationID, bestFitnessEver.ToString("0.###"))); //Snake.currentBestScore.ToString("0.###")));
				Console.ForegroundColor = ConsoleColor.White;
				bestScore = bestFitnessEver;
			}
			else
			{
				Console.Write(".");
			}

			// Check if all snakes are dead
			bool allSnakesDead = this.allSnakes.Count == 0 ? true : false;

			int counter = 0;
			foreach (Snake s in this.allSnakes)
			{
				if (!s.IsDead)
					++counter;
			}
			// Move each of the snakes in separate thread
			using (CountdownEvent threadCounter = new CountdownEvent(counter))
			{
				foreach (Snake s in this.allSnakes)
				{
					if (!s.IsDead)
					{
						ThreadPool.QueueUserWorkItem(_ => SnakeMoveCallback(s, threadCounter));
					}
				}
				threadCounter.Wait();
			}

			allSnakesDead = true;
			foreach (Snake s in this.allSnakes)
			{
				if (!s.IsDead)
				{
					allSnakesDead = false;
					break;
				}
			}

			// All snakes died so we can move onto next epoch
			if (allSnakesDead)
			{
				//Snake.CurrentBestScore = 0;
				int randomSeed = rnd.Next();

				// Remove snakes (all are dead at the moment)
				float max = 0.0f;
				foreach (Snake s in this.allSnakes)
				{
					if ((run % numberOfRuns) == 0)
					{
						s.Genome.Fitness /= numberOfRuns;
						if (s.Genome.Fitness > Snake.AllSnakesBestScore)
						{
							Snake.AllSnakesBestScore = (float)s.Genome.Fitness;
						}
						if (s.Genome.Fitness > max) max = (float)s.Genome.Fitness;
					}
				}
				if ((run % numberOfRuns) == 0)
				{
					float avg = 0.0f;
					foreach (Genome gen in this.sim.Species[0].Genomes)
					{
						avg += (float)gen.Fitness;
					}
					avg /= this.sim.Species[0].Genomes.Count;
					if (avg > this.snakeBestAvg)
					{
						this.snakeBestAvg = avg;
					}
					this.avgFitnesses.Add(avg);
				}

				this.allSnakes.Clear();

				// Go to the next epoch of the simulation
				if (!firstTime && ((run % numberOfRuns) == 0))
				{
					sim.Epoch();
					++generationID;
				}

				for (int i = 0; i < sim.Species.Count; ++i)
				{
					Species s = sim.Species[i];
					List<Genome> genomes = s.Genomes;

					for (int j = 0; j < genomes.Count; ++j)
					{
						Genome gen = genomes[j];
						if ((run % numberOfRuns) == 0)
							gen.Fitness = 0.0;

						NEAT_CSharp.Network net = gen.GetNetwork();
                        //net.ActivationFunction = new NEAT_CSharp.ActivationFunctions.ReLU();
                        //net.ActivationFunction = new NEAT_CSharp.ActivationFunctions.Threshold();

                        // Store "champion" of the generation in a file - for potential further use
                        if (i == 0 && j == 0 && ((run % numberOfRuns) == 0))
						{
							// Serialize the networks now
							//net.SerializeBinary(string.Format("{0}/best_solution_gen_{1}", dirName, generationID));
							// Also serialize to a different file if that's the best solution ever found
							if (gen.OriginalFitness > bestFitnessEver)
							{
								bestFitnessEver = gen.OriginalFitness;
								if (!this.saveAllBestSnakes) net.SaveNetworkToFile("./best_snake");
								else { net.SaveNetworkToFile($"./best_snake_gen_{this.generationID:D8}_fitness_{bestFitnessEver:0.000}"); }
							}
						}

						// Create new snake object
						// TOOD: maybe we should just "reset" existing ones?
						Snake snakeObj = new Snake();
						snakeObj.Brain = net;
						snakeObj.Genome = gen;
						snakeObj.rnd = rnd;
						snakeObj.RunID = (run % numberOfRuns);
						snakeObj.Seed = this.useNonRandomSeed ? 9 : rnd.Next();
						snakeObj.Start();

						allSnakes.Add(snakeObj);
					}

					if (firstTime) firstTime = false;
				}

				// Finally increase run counter
				run++;
			}
		}
	}
}
