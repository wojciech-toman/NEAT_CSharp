using NEAT_CSharp;
using System;
using System.Collections.Generic;

namespace NEAT_CSharp.Demo
{
    public struct Point
    {
        public int x;
        public int y;
    }


    public class Snake
	{
		public const int gridWidth = 30;
		public const int gridHeight = 30;

		public const int borderTop = -gridHeight / 2;
		public const int borderBottom = gridHeight / 2;
		public const int borderLeft = -gridWidth / 2;
		public const int borderRight = gridWidth / 2;

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

		public NEAT_CSharp.Network Brain { get; set; }
		public NEAT_CSharp.Genome Genome { get; set; }
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

			this.SpawnFood();

			// Add tail objects if starting length > 1
			if (Snake.StartingLength > 1)
			{
				Point pos = currentLocation;
				for (int i = 1; i < Snake.StartingLength; ++i)
				{
					pos.x -= vecDirection.x; pos.y -= vecDirection.y;
					tails.Add(pos);
				}
			}
		}

		private float previousDistanceToFood = 1000;

		public Point GetDirectionVector(int direction, Point oldDir)
		{
			Point newDir;
			// Move in a new Direction?
			if (direction == 1)
			{
				newDir.x = -oldDir.y;
				newDir.y = oldDir.x;
			}
			else if (direction == -1)
			{
				newDir.x = oldDir.y;
				newDir.y = -oldDir.x;
			}
			else
			{
				newDir.x = oldDir.x;
				newDir.y = oldDir.y;
			}

			return newDir;
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
			maxDistance = Math.Max(distX, distY);
			for (int i = 0; i < input.Length; ++i)
				input[i] = maxDistance;

			Point vecLeft = this.GetDirectionVector(-1, vecDirection);
			Point vecStraight = this.GetDirectionVector(0, vecDirection);
			Point vecRight = this.GetDirectionVector(1, vecDirection);

			Point[] vectors = new Point[] { vecLeft, vecStraight, vecRight };

			// Calculate the distance to food, walls and self
			for (int i = 0; i < vectors.Length; ++i)
			{
				Point currentPos = this.currentLocation;


				while (currentPos.x > borderLeft && currentPos.x < borderRight && currentPos.y > borderTop && currentPos.y < borderBottom)
				{
					currentPos.x += vectors[i].x; currentPos.y += vectors[i].y;

					// Distance to walls
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

					// Distance to self
					foreach (Point tail in this.tails)
					{
						if (tail.x == currentPos.x && tail.y == currentPos.y)
						{
							input[i + 4] = Math.Min(input[i + 4], Math.Abs(currentLocation.x - tail.x) + Math.Abs(currentLocation.y - tail.y));
						}
					}

					// Distance to food
					if (currentPos.x == foodLocation.x && currentPos.y == foodLocation.y)
					{
						//float dist = (float)Math.Sqrt(diffX * diffX + diffY * diffY);
						input[i + 7] = Math.Abs(foodLocation.x - currentLocation.x) + Math.Abs(foodLocation.y - currentLocation.y);
					}
				}
			}

			// Normalize the input [0; 1]
			for (int i = 0; i < input.Length; ++i)
			{
				input[i] /= maxDistance;
			}

			// Set bias node always to 1
			input[3] = 1.0f;


			// Activate the neural network
			this.Brain.SetInput(input);
            //net.ActivationFunction = new NEAT_CSharp.ActivationFunctions.Threshold();
            bool isActivated = this.Brain.Activate();
			if (!isActivated)
			{
				this.MarkAsDead();
				if (this.Genome != null)
					this.Genome.Fitness = 0;
				return;
			}

			List<Node> outputs = this.Brain.Output;
			float outLeft = outputs[0].Activation;			// Left
			float outStraight = outputs[1].Activation;		// Straight
			float outRight = outputs[2].Activation;			// Right
			int dir = -10;
			if (outLeft > outRight) dir = outLeft > outStraight ? -1 : 0;
			else dir = outRight > outStraight ? 1 : 0;

			this.vecDirection = GetDirectionVector(dir, this.vecDirection);

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

				// Reset the flag and spawn new food item
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

				// Move last Tail Element to where the Head was
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
			if (this.IsDead) return;


			if (this.Genome != null)
			{
				this.Genome.Fitness += Math.Pow(Math.Max(1.0f - previousDistanceToFood / maxDistance, 0.0f), 2);
			}
			this.IsDead = true;
		}
	}
}
