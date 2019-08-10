using System;
using System.Collections.Generic;

using NeuralEvolution;

namespace NeuralEvolutionDemo
{
	using NeuralEvolution;
	using System;
	using System.Collections;
	using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    public struct Point
	{
		public int x;
		public int y;
	}

	public class SnakeSimulation
	{
		Simulation sim;
		System.Random rnd = new System.Random();

		private int numberOfRuns = 1;


		Dictionary<Species, List<float>> fitnessDict = new Dictionary<Species, List<float>>();

		List<float> bestFitnesses = new List<float>();
		List<float> avgFitnesses = new List<float>();
		float snakeBestAvg = 0.0f;

		List<int> speciesCount = new List<int>();

		// Use this for initialization
		public void Start()
		{
			this.InitSimulation();

			this.isSimulationStarted = true;
		}

		string dirName;
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
				startGenome.addNode(new Node(i != 3 ? Node.ENodeType.SENSOR : Node.ENodeType.BIAS, i + 1));

			// Output: Move left, right, straight
			int outputStart = inputs + 1;
			startGenome.addNode(new Node(Node.ENodeType.OUTPUT, outputStart + 0));
			startGenome.addNode(new Node(Node.ENodeType.OUTPUT, outputStart + 1));
			startGenome.addNode(new Node(Node.ENodeType.OUTPUT, outputStart + 2));


			// Food distance
			startGenome.addConnection(1, outputStart + 0, 0.0f);
			startGenome.addConnection(2, outputStart + 2, 0.0f);
			startGenome.addConnection(3, outputStart + 1, 0.0f);

			// Tails distance
			startGenome.addConnection(5, outputStart + 0, 0.0f);
			startGenome.addConnection(6, outputStart + 2, 0.0f);
			startGenome.addConnection(7, outputStart + 1, 0.0f);

			// Walls distance
			startGenome.addConnection(8, outputStart + 0, 0.0f);
			startGenome.addConnection(9, outputStart + 2, 0.0f);
			startGenome.addConnection(10, outputStart + 1, 0.0f);

			// Connect bias node to every output
			startGenome.addConnection(4, outputStart + 0, 0.0f);
			startGenome.addConnection(4, outputStart + 1, 0.0f);
			startGenome.addConnection(4, outputStart + 2, 0.0f);


			// Start simulation
			sim = new Simulation(rnd, startGenome, 150);
			sim.Parameters.AddNodeProbability = 0.02f;
			sim.Parameters.AddConnectionProbability = 0.1f;
			sim.Parameters.WeightMutationPower = 1.0f;
			sim.Parameters.MutateConnectionWeightsProbability = 0.8f;
			sim.Parameters.MutateToggleEnabledProbability = 0.05f;
			sim.Parameters.MutateReenableProbability = 0.025f;
			sim.Parameters.AreConnectionWeightsCapped = true;
			sim.Parameters.MaxWeight = 2.0f;
			sim.Parameters.MaxSpeciesGenerationsWithoutImprovement = 70;
			sim.Parameters.MaxGeneralGenerationsWithoutImprovement = 80;
		}

		// Update is called once per frame
		public void Update()
		{
			if (isSimulationStarted)
				Sim();
		}

		//List<NeuralEvolution.Network> bestNetworks = new List<NeuralEvolution.Network>();

		private int generationID = 1;
		private List<Snake> allSnakes = new List<Snake>();
		bool firstTime = true;
		bool isSimulationStarted = false;
		double bestFitnessEver = 0.0f;
		int run = 0;

		private static void snakeMoveCallback(Snake snake, CountdownEvent evt)
		{
			snake.Move();
			evt.Signal();
		}

		double bestScore = -1;

		public int GenerationID { get => generationID; }

		void Sim()
		{
			// Print the best result so far
			if (bestFitnessEver > bestScore)
			{
				Console.WriteLine("");
				Console.WriteLine(System.String.Format("Generation: {0} | Current best: {1}", this.generationID, bestFitnessEver.ToString("0.###"))); //Snake.currentBestScore.ToString("0.###")));
				bestScore = bestFitnessEver;
			} else
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
						ThreadPool.QueueUserWorkItem(_ => snakeMoveCallback(s, threadCounter));
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
					foreach (Genome gen in this.sim.Species[0].getGenomes())
					{
						avg += (float)gen.Fitness;
					}
					avg /= this.sim.Species[0].getGenomes().Count;
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
					sim.epoch();
					++generationID;
				}

				//this.bestNetworks.Clear();

				for (int i = 0; i < sim.Species.Count; ++i)
				{
					Species s = sim.Species[i];
					List<Genome> genomes = s.getGenomes();

					for (int j = 0; j < genomes.Count; ++j)
					{
						Genome gen = genomes[j];
						if ((run % numberOfRuns) == 0)
							gen.Fitness = 0.0;

						NeuralEvolution.Network net = gen.getNetwork();

						// Visualize best performer from each species
						//if (j == 0)
						//{
						//	this.bestNetworks.Add(net);
						//}

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

	public class Snake
	{
		const int borderTop = -15;
		const int borderBottom = 15;
		const int borderLeft = -15;
		const int borderRight = 15;

		//public static int GlobalSnakeID = 0;
		//private int SnakeID;
		//private string foodName;

		private static float allSnakesBestScore = 0;
		private static float currentBestScore = 0;

		private int sinceLastUpdate = 0;

		float fitness = 0;

		
		// Current Movement Direction (by default it moves to the right)
		private Point vecDirection;

		// Keep Track of Tail
		private List<Point> tails = new List<Point>();
		// Did the snake eat something?
		private bool ate = false;

		public NeuralEvolution.Network Brain { get; set; }
		public NeuralEvolution.Genome Genome { get; set; }
		public bool IsDead { get; set; }

		public System.Random rnd;

		private Point currentLocation;
		private Point foodLocation;

		// Starting length of the snake
		public static int StartingLength { get; set; } = 1;

		void SpawnFood()
		{
			int newX = rnd.Next(borderLeft + 1, borderRight);
			int newY = rnd.Next(borderTop + 1, borderBottom);

			foodLocation.x = newX;
			foodLocation.y = newY;

			this.previousDistanceToFood = 200.0f;

			int xDiff = (foodLocation.x - currentLocation.x);
			int yDiff = (foodLocation.y - currentLocation.y);
			float distance = (float)Math.Sqrt(xDiff * xDiff + yDiff * yDiff);
			if (distance < previousDistanceToFood)
				previousDistanceToFood = distance;
		}

		private void OnDestroy()
		{
			tails.Clear();
		}
		public int Seed { get; set; }

		public void Start()
		{
			vecDirection.x = 1;
			vecDirection.y = 0;

			// Below is using given seed to make sure each snake has same conditions
			this.rnd = new System.Random(Seed);

			// Start in random direction
			int startDir = rnd.Next(3);
			if (startDir == 0) { vecDirection.x = 0; vecDirection.y = -1; }
			if (startDir == 1) { vecDirection.x = 0; vecDirection.y = 1; }
			if (startDir == 2) { vecDirection.x = -1; vecDirection.y = 0; }
			if (startDir == 3) { vecDirection.x = 1; vecDirection.y = 0; }

			this.SpawnFood();

			// Add tail objects if starting length > 1
			if (Snake.StartingLength > 1)
			{
				Point pos = currentLocation;
				for (int i = 1; i < Snake.StartingLength; ++i)
				{
					pos.x -= vecDirection.x; pos.y -= vecDirection.y;
					//this.AddTailObject(pos);
					tails.Add(pos);

					// Move last Tail Element to wher.e the Head was
					/*tails[tails.Count - 1] = pos;

					// Add to front of list, remove from the back
					tails.Insert(0, tails[tails.Count - 1]);
					tails.RemoveAt(tails.Count - 1);*/
				}
			}
		}

		private float previousDistanceToFood = 1000;

		public void SetDirection(int direction)
		{
			Point newDir;
			// Move in a new Direction?
			if (direction == 1)
			{
				newDir.x = -vecDirection.y;
				newDir.y = vecDirection.x;
			}
			else if (direction == -1)
			{
				newDir.x = vecDirection.y;
				newDir.y = -vecDirection.x;
			}
			else
			{
				newDir.x = vecDirection.x;
				newDir.y = vecDirection.y;
			}

			vecDirection.x = newDir.x;
			vecDirection.y = newDir.y;
		}

		float maxDistance;

		public int WithoutChangesThreshold { get; set; } = 240;
		public int RunID { get; internal set; }
		public static float AllSnakesBestScore { get => allSnakesBestScore; set => allSnakesBestScore = value; }
		public static float CurrentBestScore { get => currentBestScore; set => currentBestScore = value; }
		public Point FoodLocation { get => foodLocation; set => foodLocation = value; }
		public Point CurrentLocation { get => currentLocation; set => currentLocation = value; }
		public List<Point> Tails { get => tails; set => tails = value; }

		float topScore = 0.0f;          // Maximum fitness for this snake

		private void MovementIteration()
		{
			++sinceLastUpdate;

			// "Thinking" phase
			int inputs = 3 + 3 + 3 + 1;
			float[] input = new float[inputs];

			float distX = borderRight - borderLeft;
			float distY = borderBottom - borderTop;
			maxDistance = Math.Max(distX, distY);// (float)Math.Sqrt(distX * distX + distY * distY);
			for (int i = 0; i < input.Length; ++i)
				input[i] = maxDistance;

			Point vecLeft; vecLeft.x = -vecDirection.y; vecLeft.y = vecDirection.x;
			Point vecStraight; vecStraight.x = vecDirection.x; vecStraight.y = vecDirection.y;
			Point vecRight; vecRight.x = vecDirection.y; vecRight.y = -vecDirection.x;

			Point[] vectors = new Point[] { vecLeft, vecStraight, vecRight };
			for (int i = 0; i < vectors.Length; ++i)
			{
				Point currentPos = this.currentLocation;


				while (currentPos.x > borderLeft && currentPos.x < borderRight && currentPos.y > borderTop && currentPos.y < borderBottom)
				{
					currentPos.x += vectors[i].x; currentPos.y += vectors[i].y;

					if (currentPos.x == borderLeft)
					{
						input[i] = Math.Abs(currentLocation.x - currentPos.x); break;
					}
					else if (currentPos.x == borderRight)
					{
						input[i] = Math.Abs(currentLocation.x - currentPos.x); break;
					}
					else if (currentPos.y == borderTop)
					{
						input[i] = Math.Abs(currentLocation.y - currentPos.y); break;
					}
					else if (currentPos.y == borderBottom)
					{
						input[i] = Math.Abs(currentLocation.y - currentPos.y); break;
					}

					foreach (Point tail in this.tails)
					{
						if (tail.x == currentPos.x && tail.y == currentPos.y)
						{
							input[i + 4] = Math.Abs(currentLocation.x - tail.x) + Math.Abs(currentLocation.y - tail.y);
						}
					}

					if (currentPos.x == foodLocation.x && currentPos.y == foodLocation.y)
					{
						//float dist = (float)Math.Sqrt(diffX * diffX + diffY * diffY);
						input[i + 7] = Math.Abs(foodLocation.x - currentLocation.x) + Math.Abs(foodLocation.y - currentLocation.y);
					}
				}
			}

			/*if (vecDirection.x == -1 && vecDirection.y == 0)
			{
				input[0] = (foodLocation.x == currentLocation.x && foodLocation.y > currentLocation.y) ? input[0] = Math.Abs(currentLocation.y - foodLocation.y) : maxDistance;
				input[1] = (foodLocation.x < currentLocation.x && foodLocation.y == currentLocation.y) ? input[1] = Math.Abs(currentLocation.x - foodLocation.x) : maxDistance;
				input[2] = (foodLocation.x == currentLocation.x && foodLocation.y < currentLocation.y) ? input[2] = Math.Abs(currentLocation.y - foodLocation.y) : maxDistance;

				// TODO: Add input[4] to input[6] as a distance to snake itself

				input[7] = Math.Abs(borderBottom - currentLocation.y);
				input[8] = Math.Abs(borderLeft - currentLocation.x);
				input[9] = Math.Abs(borderTop - currentLocation.y);
			}
			else if (vecDirection.x == 1 && vecDirection.y == 0)
			{
				input[0] = (foodLocation.x == currentLocation.x && foodLocation.y < currentLocation.y) ? input[0] = Math.Abs(currentLocation.y - foodLocation.y) : maxDistance;
				input[1] = (foodLocation.x < currentLocation.x && foodLocation.y == currentLocation.y) ? input[1] = Math.Abs(currentLocation.x - foodLocation.x) : maxDistance;
				input[2] = (foodLocation.x == currentLocation.x && foodLocation.y > currentLocation.y) ? input[2] = Math.Abs(currentLocation.y - foodLocation.y) : maxDistance;

				// TODO: Add input[4] to input[6] as a distance to snake itself

				input[7] = Math.Abs(borderTop - currentLocation.y);
				input[8] = Math.Abs(borderRight - currentLocation.x);
				input[9] = Math.Abs(borderBottom - currentLocation.y);
			}
			else if (vecDirection.x == 0 && vecDirection.y == 1)
			{
				input[0] = (foodLocation.y == currentLocation.y && foodLocation.x > currentLocation.x) ? input[0] = Math.Abs(currentLocation.x - foodLocation.x) : maxDistance;
				input[1] = (foodLocation.x == currentLocation.x && foodLocation.y > currentLocation.y) ? input[1] = Math.Abs(currentLocation.y - foodLocation.y) : maxDistance;
				input[2] = (foodLocation.y == currentLocation.y && foodLocation.x < currentLocation.x) ? input[2] = Math.Abs(currentLocation.x - foodLocation.x) : maxDistance;

				// TODO: Add input[4] to input[6] as a distance to snake itself

				input[7] = Math.Abs(borderRight - currentLocation.x);
				input[8] = Math.Abs(borderBottom - currentLocation.y);
				input[9] = Math.Abs(borderLeft - currentLocation.x);
			}
			else if (vecDirection.x == 0 && vecDirection.y == -1)
			{
				input[0] = (foodLocation.y == currentLocation.y && foodLocation.x < currentLocation.x) ? input[0] = Math.Abs(currentLocation.x - foodLocation.x) : maxDistance;
				input[1] = (foodLocation.x == currentLocation.x && foodLocation.y < currentLocation.y) ? input[1] = Math.Abs(currentLocation.y - foodLocation.y) : maxDistance;
				input[2] = (foodLocation.y == currentLocation.y && foodLocation.x > currentLocation.x) ? input[2] = Math.Abs(currentLocation.x - foodLocation.x) : maxDistance;

				// TODO: Add input[4] to input[6] as a distance to snake itself

				input[7] = Math.Abs(borderLeft - currentLocation.x);
				input[8] = Math.Abs(borderTop - currentLocation.y);
				input[9] = Math.Abs(borderRight - currentLocation.x);
			}*/





			for (int i = 0; i < input.Length; ++i)
			{
				input[i] /= maxDistance;
			}

			// Set bias node always to 1
			input[3] = 1.0f;


			// Activate the neural network
			this.Brain.setInput(input);
			bool isActivated = this.Brain.activate();
			if (!isActivated)
			{
				this.MarkAsDead();
				if (this.Genome != null)
					this.Genome.Fitness = 0;
				return;
			}

			List<Node> outputs = this.Brain.Output;
			float out1 = outputs[0].Activation;
			float out2 = outputs[1].Activation;
			float out3 = outputs[2].Activation;
			int dir = -10;
			if (out1 > out2) dir = out1 > out3 ? -1 : 0;
			else dir = out2 > out3 ? 1 : 0;

			this.SetDirection(dir);

			// Save current position (gap will be here)
			Point v = this.currentLocation;

			// Move head into new direction (now there is a gap)
			currentLocation.x += vecDirection.x; currentLocation.y += vecDirection.y;

			if (currentLocation.x == foodLocation.x && currentLocation.y == foodLocation.y)
				ate = true;

			// Did eat something? If so, insert new tail element into the gap and increase snake's genome fitness (to reward it for doing good thing)
			if (ate)
			{
				fitness += 1;
				if (this.Genome != null)
				{
					this.Genome.Fitness += 1;
					float score = (float)(this.Genome.Fitness / (this.RunID + 1));
					if (score > Snake.CurrentBestScore)
						CurrentBestScore = score;
				}

				this.AddTailObject(v);

				sinceLastUpdate = 0;

				// Reset the flag
				ate = false;
				SpawnFood();
			}
			// Do we have a Tail?
			else if (tails.Count > 0)
			{
				if (currentLocation.x == borderLeft || currentLocation.x == borderRight || currentLocation.y == borderTop || currentLocation.y == borderBottom)
				{
					this.MarkAsDead();
					return;
				}

				for (int i = 0; i < tails.Count; ++i)
				{
					if (tails[i].x == currentLocation.x && tails[i].y == currentLocation.y)
					{
						this.MarkAsDead();
						return;
					}
				}

				// Move last Tail Element to wher.e the Head was
				tails[tails.Count - 1] = v;

				// Add to front of list, remove from the back
				tails.Insert(0, tails[tails.Count - 1]);
				tails.RemoveAt(tails.Count - 1);
			}

			// Incur small fitness penalty for every move away from food
			float newDistanceToFood = 200.0f;

			int xDiff = (foodLocation.x - currentLocation.x);
			int yDiff = (foodLocation.y - currentLocation.y);
			float distance = (float)Math.Sqrt(xDiff * xDiff + yDiff * yDiff);
			if (distance < newDistanceToFood)
				newDistanceToFood = distance;


			previousDistanceToFood = newDistanceToFood;


			if (fitness > this.topScore)
				topScore = fitness;
		}

		public void MoveOnce()
		{
			// Died already - so don't move it
			if (this.IsDead)
				return;

			this.MovementIteration();

			if (sinceLastUpdate >= WithoutChangesThreshold)
				this.MarkAsDead();
		}
		public void Move()
		{
			// Died already - so don't move it
			if (this.IsDead)
				return;

			while (!this.IsDead && sinceLastUpdate < WithoutChangesThreshold)
			{
				this.MovementIteration();
			}

			this.MarkAsDead();
		}

		private void AddTailObject(Point pos)
		{
			// Keep track of it in our tail list
			tails.Insert(0, pos);
		}

		private void MarkAsDead()
		{
			// Make sure fitness isn't negative (it can be at that point if snake was moving away from food most of the time)
			//if (fitness < 0) fitness = 0;
			if (this.IsDead) return;


			if (this.Genome != null)
			{
				this.Genome.Fitness += Math.Pow(Math.Max(1.0f - previousDistanceToFood / maxDistance, 0.0f), 2);
			}
			this.IsDead = true;
		}
	}



	// Basic demo program checking basic functions of the network
	class Program
	{
		static void Main(string[] args)
		{
			Random r = new Random();

			Console.WriteLine("Tests of NEAT. Press:");
			Console.WriteLine("- 1 to run XOR test");
			Console.WriteLine("- 2 to run Pole Balancing (single) test");
			Console.WriteLine("- 3 to run Snake Game test (prints only results)");
			Console.WriteLine("- 4 to run Snake Game replay (using network saved in prior simulation)");
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
				else if (key.Key == ConsoleKey.D3)
					SnakeGameTest(r);
				else if (key.Key == ConsoleKey.D4)
					SnakeGameReplay(r);
				else if (key.Key == ConsoleKey.D0)
					SystemTests(r);
				else
					Console.WriteLine("Test not found");
			}
		}

		// TODO: move to unit tests
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


		private static void SnakeGameTest(Random r)
		{
			SnakeSimulation sim = new SnakeSimulation();
			sim.Start();
			// Run 250 epochs
			while (sim.GenerationID < 250)
			{
				sim.Update();
			}
			Console.WriteLine("\nSimulation finished");
		}

		private static void SnakeGameReplay(Random r)
		{
			if (!File.Exists("./best_snake"))
			{
				Console.WriteLine("\nNo saved networks found. Please make sure to run simulation first");
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
			Console.SetWindowSize(32, 32);
			Console.Clear();
			for (int y = 0; y <= 30; ++y)
			{
				for (int x = 0; x <= 30; ++x)
				{
					if (x == 0 || x == 30 || y == 0 || y == 30)
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
				if (previousSnakeLocation.x != -100 && previousSnakeLocation.y != 10 &&
					previousSnakeLocation.x != snakeObj.FoodLocation.x && previousSnakeLocation.y != snakeObj.FoodLocation.y)
				{
					Console.SetCursorPosition(previousSnakeLocation.x + 15, previousSnakeLocation.y + 15);
					Console.Write(" ");
				}
				previousSnakeLocation = snakeObj.FoodLocation;

				pointsToDraw.Clear();
				pointsToDraw.Add(snakeObj.CurrentLocation);
				pointsToDraw.AddRange(snakeObj.Tails);

				if (previousSnake.Count > 0)
				{
					List<Point> pointsToClear = new List<Point>();
					foreach(Point p in previousSnake)
					{
						if (!pointsToDraw.Contains(p))
						{
							pointsToClear.Add(p);
							pointsToDraw.Remove(p);
						}
					}
					foreach(Point p in pointsToClear)
					{
						Console.SetCursorPosition(p.x + 15, p.y + 15);
						Console.Write(" ");
					}
				}

				previousSnake.Clear();
				previousSnake.Add(snakeObj.CurrentLocation);
				previousSnake.AddRange(snakeObj.Tails);

				// Draw current food and snake
				Console.ForegroundColor = ConsoleColor.Red;
				Console.SetCursorPosition(snakeObj.FoodLocation.x + 15, snakeObj.FoodLocation.y + 15);
				Console.Write("O");

				Console.ForegroundColor = ConsoleColor.Green;
				foreach (Point p in pointsToDraw)
				{
					Console.SetCursorPosition(p.x + 15, p.y + 15);
					Console.Write("@");
				}

				Console.SetCursorPosition(0, 0);

				// Move the snake
				snakeObj.MoveOnce();

				Thread.Sleep(10);
			}
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

				// We found a solution so we can exit already
				if (solutionFound)
					break;

				// Advance to the next step of simulation
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
