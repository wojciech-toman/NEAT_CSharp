using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT_CSharp
{
	// TODO: add 'elitism' - option to move more than one of the best genomes of the species to the next generation unchanged
	[Serializable]
	// Species is basically a collection of genomes sharing some common features ('distance'). Each species have Champion - the best performing
	// Genome in the previous epoch of simulation.
	public class Species
	{
		private List<Genome> genomes = new List<Genome>();
		private Genome champion = null;
		private Random random = null;

		public int Age { get; set; }

		// Population to which this species belongs
		public Simulation ParentSimulation { get; set; }

		public int Offspring { get; set; }
		public int AgeWithoutImprovement { get { return this.Age - this.LastImprovementAge; } }
		public int LastImprovementAge { get; set; }
		public bool ShouldBePenalized { get; set; }
		// Maximum property ever registered by this species
		public double MaxFitnessEver { get; private set; } = -1.0;

		public List<Innovation> Innovations { get; set; }
		public int ChampionOffspring { get; internal set; }

		public Species(Random rnd)
		{
			random = rnd;
		}

		public void AddGenome(Genome gen)
		{
			genomes.Add(gen);
		}

		// Remove given genome
		public void RemoveGenome(Genome gen)
		{
			genomes.Remove(gen);
		}

		/*public double getAdjustedFitness(int genomeIdx)
		{
			return (this.genomes[genomeIdx].Fitness / this.genomes.Count);
		}*/

		public double GetAverageFitness()
		{
			double totalFitness = 0.0;
			for (int i = 0; i < this.genomes.Count; ++i)
				totalFitness += this.genomes[i].Fitness;

			return (totalFitness / this.genomes.Count);
		}

		/*public double getTotalAdjustedFitness()
		{
			double totalFitness = 0.0;
			for (int i = 0; i < this.genomes.Count; ++i)
				totalFitness += this.getAdjustedFitness(i);

			return totalFitness;
		}*/

		public Genome GetSampleGenome()
		{
			return this.genomes[0];
		}

        public List<Genome> Genomes => this.genomes;

        // Looks for a best performing genome in the genomes list
        public Genome GetChampion()
		{
			double maxFitness = -1.0;
			this.champion = null;

			foreach (Genome gene in this.genomes)
			{
				if (gene.Fitness > maxFitness)
				{
					maxFitness = gene.Fitness;
					this.champion = gene;
				}
			}

			return this.champion;
		}

		// Performs reproduction of organisms in the genomes list
		public void Reproduce(List<Genome> nextGeneration, List<Innovation> innovations)
		{
			if (nextGeneration == null) throw new ArgumentNullException(nameof(nextGeneration));

			if (this.Offspring <= 0) return;

			bool championCloned = false;

			for (int i = 0; i < this.Offspring; ++i)
			{
				Genome child = null;
				if (this.ChampionOffspring > 0)
				{
					child = this.GetChampion().Copy();
					child.ParentSimulation = this.ParentSimulation;
					if (this.ChampionOffspring > 1)
					{
						child.IsPopulationChampion = false;
						child.Fitness = 0.0;
						child.OriginalFitness = 0.0;
						if (this.random.NextDouble() < this.ParentSimulation.Parameters.MutateConnectionWeightsProbability)
							child.MutateWeights(this.ParentSimulation.Parameters.WeightMutationPower);
						else
						{
							child.AddConnectionMutation(innovations);
						}
					}
					else
						child.IsPopulationChampion = true;

					--this.ChampionOffspring;
				}
				else if (!championCloned && this.Offspring > 5)
				{
					child = this.GetChampion().Copy();
					child.ParentSimulation = this.ParentSimulation;
					child.IsPopulationChampion = true;
					championCloned = true;
				}
				else
				{
					// Mutate only (without crossover)
					if (this.random.NextDouble() < this.ParentSimulation.Parameters.MutateWithoutCrossover)
					{
						// Select random genome to mutate
						child = this.genomes[this.random.Next(this.genomes.Count)].Copy();
						child.ParentSimulation = this.ParentSimulation;
						child.Fitness = 0.0;
						child.OriginalFitness = 0.0;
						child.IsPopulationChampion = false;
						this.MutateChild(child);
					}
					// Perform crossover
					else
					{
						// Select random parents
						Genome parent1 = this.genomes[this.random.Next(this.genomes.Count)];
						Genome parent2 = null;

						// Find second parent INSIDE this species
						if (this.random.NextDouble() > this.ParentSimulation.Parameters.InterspeciesMateRate)
						{
							parent2 = this.genomes[this.random.Next(this.genomes.Count)];
						}
						// Find second parent OUTSIDE this species - this should happen very very rarely
						else
						{
							// TODO: possible modification - select only from better performing species
							Species speciesFound = this;
							int tries = 0;
							while (speciesFound == this && tries++ < 5)
							{
								speciesFound = this.ParentSimulation.Species[this.random.Next(this.ParentSimulation.Species.Count)];
							}
							if (speciesFound.Genomes.Count == 0) speciesFound = this;

							parent2 = speciesFound.Genomes[this.random.Next(speciesFound.Genomes.Count)];
						}

						if (this.random.NextDouble() > this.ParentSimulation.Parameters.AverageCrossoverProbability)
							child = parent1.Crossover(parent2, this.random);
						else
							child = parent2.CrossoverAverage(parent2, this.random);

						// Mutate child
						if (this.random.NextDouble() > this.ParentSimulation.Parameters.MateWithoutMutatingProbability || parent1 == parent2 || parent1.CompatibilityDistance(parent2) < 0.00001f)
						{
							this.MutateChild(child);
						}
					}
				}

				if (child != null)
					nextGeneration.Add(child);
			}
		}

		private void MutateChild(Genome child)
		{
			if (this.random.NextDouble() < this.ParentSimulation.Parameters.AddNodeProbability)
			{
				child.AddNodeMutation(this.Innovations);
			}
			else if (this.random.NextDouble() < this.ParentSimulation.Parameters.AddConnectionProbability)
			{
				child.AddConnectionMutation(this.Innovations);
			}
			else
			{
				if (this.random.NextDouble() < this.ParentSimulation.Parameters.MutateConnectionWeightsProbability)
				{
					child.MutateWeights(this.ParentSimulation.Parameters.WeightMutationPower);
				}
				if (this.random.NextDouble() < this.ParentSimulation.Parameters.MutateToggleEnabledProbability)
				{
					child.ToggleEnabledMutation();
				}
				if (this.random.NextDouble() < this.ParentSimulation.Parameters.MutateReenableProbability)
				{
					child.ReenableMutation();
				}
			}
		}

		public void AdjustFitness()
		{
			// First adjust fitness
			int ageDebt = (this.Age - this.LastImprovementAge + 1) - this.ParentSimulation.Parameters.MaxSpeciesGenerationsWithoutImprovement;

			if (ageDebt == 0)
				ageDebt = 1;

			// TODO: add fitness boost to the young species?
			foreach (Genome gen in this.genomes)
			{
				gen.OriginalFitness = gen.Fitness;

				// Penalize extreme stagnation
				if ((ageDebt >= 1) || this.ShouldBePenalized)
				{
					gen.Fitness *= this.ParentSimulation.Parameters.SpeciesStagnationPenalty;
				}

				double epsilon = 0.00001;
				if (gen.Fitness < epsilon) gen.Fitness = epsilon;

				// Share fitness with the species
				gen.Fitness /= this.genomes.Count;
			}

			this.OrderGenomes();

			// If current best fitness is greater than maximum fitness ever found, there's improvement
			if (this.genomes[0].OriginalFitness > MaxFitnessEver)
			{
				this.MaxFitnessEver = this.genomes[0].OriginalFitness;
				this.LastImprovementAge = this.Age;
			}

			int numberOfParents = (int)(this.genomes.Count * this.ParentSimulation.Parameters.SurvivalThreshold + 1.0f);

			// Mark genomes with least fitness for deletion.
			for (int i = numberOfParents; i < this.genomes.Count; ++i)
				this.genomes[i].ShouldBeEliminated = true;
		}

		public void OrderGenomes()
		{
			// Order genomes by decreasing fitness
			this.genomes.Sort((x, y) => (y.Fitness.CompareTo(x.Fitness)));
		}
	}
}
