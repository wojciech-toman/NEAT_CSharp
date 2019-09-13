namespace NEAT_CSharp.Demo
{
    using NEAT_CSharp;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public class SnakeSimulation
	{
        #region Variables

        private Simulation sim;
        private System.Random rnd = new System.Random();

		private int numberOfRuns = 1;

        private List<float> bestFitnesses = new List<float>();
        private List<float> avgFitnesses = new List<float>();

        private float snakeBestAvg = 0.0f;

        private int generationID = 1;
        private List<Snake> allSnakes = new List<Snake>();
        private bool firstTime = true;
        private bool isSimulationStarted = false;
        private double bestFitnessEver = 0.0f;
        private int run = 0;

        private double bestScore = -1;

        #endregion


        // Use this for initialization
        public void Start()
		{
			this.InitSimulation();

			this.isSimulationStarted = true;
		}

		private void InitSimulation()
		{
			Snake.StartingLength = 1;// 3;// int.Parse(this.startingSnakeLength.text);

			Genome startGenome = new Genome(rnd);
			int inputs = 3 + 1 + 3 + 3;
			// 3 nodes are used to store distance to food,
			// 1 node is used as a bias (always 1.0),
			// 3 nodes are used to store distance to the snake itself (to avoid collisions with self - hopefully),
			// 3 nodes are used to store distance to the walls
			for (int i = 0; i < inputs; ++i)
				startGenome.AddNode(new Node(i != 3 ? Node.ENodeType.SENSOR : Node.ENodeType.BIAS, i + 1));

			// Output: Move left, straight, right
			int outputStart = inputs + 1;
			startGenome.AddNode(new Node(Node.ENodeType.OUTPUT, outputStart + 0));
			startGenome.AddNode(new Node(Node.ENodeType.OUTPUT, outputStart + 1));
			startGenome.AddNode(new Node(Node.ENodeType.OUTPUT, outputStart + 2));


			// Food distance
			startGenome.AddConnection(1, outputStart + 0, 0.0f);
			startGenome.AddConnection(2, outputStart + 1, 0.0f);
			startGenome.AddConnection(3, outputStart + 2, 0.0f);

			// Tails distance
			startGenome.AddConnection(5, outputStart + 0, 0.0f);
			startGenome.AddConnection(6, outputStart + 1, 0.0f);
			startGenome.AddConnection(7, outputStart + 2, 0.0f);

			// Walls distance
			startGenome.AddConnection(8, outputStart + 0, 0.0f);
			startGenome.AddConnection(9, outputStart + 1, 0.0f);
			startGenome.AddConnection(10, outputStart + 2, 0.0f);

			// Connect bias node to every output
			startGenome.AddConnection(4, outputStart + 0, 0.0f);
			startGenome.AddConnection(4, outputStart + 1, 0.0f);
			startGenome.AddConnection(4, outputStart + 2, 0.0f);


			// Set up simulation - but don't start it
			sim = new Simulation(rnd, startGenome, 150);
			sim.Parameters.AddNodeProbability = 0.02f;
			sim.Parameters.AddConnectionProbability = 0.1f;
			sim.Parameters.WeightMutationPower = 1.0f;
			sim.Parameters.MutateConnectionWeightsProbability = 0.8f;
			sim.Parameters.MutateToggleEnabledProbability = 0.05f;
			sim.Parameters.MutateReenableProbability = 0.025f;
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
				Console.WriteLine(System.String.Format("Generation: {0} | Current best: {1}", this.generationID, bestFitnessEver.ToString("0.###"))); //Snake.currentBestScore.ToString("0.###")));
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
				Snake.CurrentBestScore = 0;
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
						//Console.WriteLine(string.Format("Average fitness: {0}", this.snakeBestAvg.ToString("0.###")));
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
								net.SerializeBinary(string.Format("./best_snake"));
							}
						}

						// Create new snake object
						// TOOD: maybe we should just "reset" existing ones?
						Snake snakeObj = new Snake();
						snakeObj.Brain = net;
						snakeObj.Genome = gen;
						snakeObj.rnd = rnd;
						snakeObj.RunID = (run % numberOfRuns);
						snakeObj.Seed = rnd.Next();
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
